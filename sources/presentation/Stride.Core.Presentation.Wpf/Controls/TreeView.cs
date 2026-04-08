// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.VisualTree;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Collections;
using Stride.Core.Presentation.Extensions;
using Stride.Core.Presentation.Internal;

namespace Stride.Core.Presentation.Controls
{
    /// <summary>
    /// Represents a control that displays hierarchical data in a tree structure that has items that can expand and collapse.
    /// </summary>
    [TemplatePart(Name = ScrollViewerPartName, Type = typeof(ScrollViewer))]
    public class TreeView : ItemsControl
    {
        /// <summary>
        /// The name of the <see cref="ScrollViewer"/> contained in this <see cref="TreeView"/>.
        /// </summary>
        public const string ScrollViewerPartName = "PART_Scroller";

        /// <summary>
        /// Identifies the <see cref="SelectedItem"/> dependency property.
        /// </summary>
        public static AvaloniaProperty SelectedItemProperty =
            AvaloniaProperty.Register<TreeView, object>(nameof(SelectedItem));
        /// <summary>
        /// Identifies the <see cref="SelectedItems"/> dependency property.
        /// </summary>
        public static AvaloniaProperty SelectedItemsProperty =
            AvaloniaProperty.RegisterDirect<TreeView, IList>(nameof(SelectedItems), o => o.SelectedItems);
        /// <summary>
        /// Identifes the <see cref="SelectionMode"/> dependency property.
        /// </summary>
        public static AvaloniaProperty SelectionModeProperty =
            AvaloniaProperty.Register<TreeView, SelectionMode>(nameof(SelectionMode), SelectionMode.Multiple, defaultBindingMode:BindingMode.TwoWay);
        /// <summary>
        /// Identifies the <see cref="IsVirtualizing"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty IsVirtualizingProperty =
            AvaloniaProperty.Register<TreeView, bool>(nameof(IsVirtualizing));

        /// <summary>
        /// Identifies the <see cref="PrepareItem"/> routed event.
        /// This attached routed event may be raised by the PropertyGrid itself or by a PropertyItemBase containing sub-items.
        /// </summary>
        public static readonly RoutedEvent PrepareItemEvent =
            RoutedEvent.Register<TreeView, RoutedEventArgs>("PrepareItem", RoutingStrategies.Bubble);
        /// <summary>
        /// Identifies the <see cref="ClearItem"/> routed event.
        /// This attached routed event may be raised by the PropertyGrid itself or by a PropertyItemBase containing sub items.
        /// </summary>
        public static readonly RoutedEvent ClearItemEvent =
            RoutedEvent.Register<TreeView, RoutedEventArgs>("ClearItem", RoutingStrategies.Bubble);

        /// <summary>
        /// Indicates whether the Control key is currently down.
        /// </summary>
        internal static bool IsControlKeyDown => (Keyboard.Modifiers & KeyModifiers.Control) == KeyModifiers.Control;

        /// <summary>
        /// Indicates whether the Shift key is currently down.
        /// </summary>
        internal static bool IsShiftKeyDown => (Keyboard.Modifiers & KeyModifiers.Shift) == KeyModifiers.Shift;

        // the space where items will be realized if virtualization is enabled. This is set by virtualizingtreepanel.
        internal VirtualizingTreePanel.VerticalArea RealizationSpace = new();
        internal VirtualizingTreePanel.SizesCache CachedSizes = new();
        private bool updatingSelection;
        private bool stoppingEdition;
        private bool allowedSelectionChanges;
        private bool pointerPressed;
        private bool scrollViewerReentrency;
        private object lastShiftRoot;
        private TreeViewItem editedItem;
        private ScrollViewer scroller;

        static TreeView()
        {
            SelectedItemProperty.Changed.AddClassHandler<AvaloniaObject>(OnSelectedItemPropertyChanged);
            SelectedItemsProperty.Changed.AddClassHandler<AvaloniaObject>(OnSelectedItemsPropertyChanged);
            SelectionModeProperty.Changed.AddClassHandler<AvaloniaObject>(OnSelectionModeChanged);
            

            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(TreeView), new (KeyboardNavigationMode.Contained));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(TreeView), new (KeyboardNavigationMode.None));
            VirtualizingPanel.ScrollUnitProperty.OverrideMetadata(typeof(TreeView), new (ScrollUnit.Item));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeView"/> class.
        /// </summary>
        public TreeView()
        {
            SelectedItems = new NonGenericObservableListWrapper<object>(new ObservableList<object>());
        }

        public bool IsVirtualizing { get { return (bool)GetValue(IsVirtualizingProperty); } set { SetValue(IsVirtualizingProperty, value.Box()); } }

        /// <summary>
        /// Gets the last selected item.
        /// </summary>
        public object SelectedItem { get { return GetValue(SelectedItemProperty); } set { SetValue(SelectedItemProperty, value); } }

        /// <summary>
        /// Gets the list of selected items.
        /// </summary>
        public IList SelectedItems { get { return (IList)GetValue(SelectedItemsProperty); } private set { SetValue(SelectedItemsProperty, value); } }

        /// <summary>
        /// Gets the selection mode for this control.
        /// </summary>
        public SelectionMode SelectionMode { get { return (SelectionMode)GetValue(SelectionModeProperty); } set { SetValue(SelectionModeProperty, value); } }

        /// <summary>
        /// Raised when a <see cref="TreeViewItem"/> is being prepared to be added to the <see cref="TreeView"/>.
        /// </summary>
        /// <seealso cref="PrepareContainerForItemOverride"/>
        public event EventHandler<TreeViewItemEventArgs> PrepareItem { add { AddHandler(PrepareItemEvent, value); } remove { RemoveHandler(PrepareItemEvent, value); } }

        /// <summary>
        /// Raised when a <see cref="TreeViewItem"/> is being cleared from the <see cref="TreeView"/>.
        /// </summary>
        /// <seealso cref="ClearContainerForItemOverride"/>
        public event EventHandler<TreeViewItemEventArgs> ClearItem { add { AddHandler(ClearItemEvent, value); } remove { RemoveHandler(ClearItemEvent, value); } }

        /// <inheritdoc/>
        /// <inheritdoc />
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            scroller = DependencyObjectExtensions.CheckTemplatePart<ScrollViewer>( e.NameScope.Find<ScrollViewer>(ScrollViewerPartName));
            if (scroller != null)
            {
                scroller.ScrollChanged += ScrollChanged;
            }
        }

        // TODO: This method has been implemented with a lot of fail and retry, and should be cleaned.
        // TODO: Also, it is probably close to work with virtualization, but it needs some testing
        public bool BringItemToView([NotNull] object item, [NotNull] Func<object, object> getParent)
        {
            // Useful link: https://msdn.microsoft.com/en-us/library/ff407130%28v=vs.110%29.aspx
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (getParent == null) throw new ArgumentNullException(nameof(getParent));
            if (IsVirtualizing)
                throw new InvalidOperationException("BringItemToView cannot be used when the tree view is virtualizing.");

            TreeViewItem container = null;

            var path = new List<object> { item };
            var parent = getParent(item);
            while (parent != null)
            {
                path.Add(parent);
                parent = getParent(parent);
            }

            for (var i = path.Count - 1; i >= 0; --i)
            {
                if (container != null)
                    container = (TreeViewItem)container.ItemContainerGenerator.ContainerFromItem(path[i]);
                else
                    container = (TreeViewItem)ItemContainerGenerator.ContainerFromItem(path[i]);

                if (container == null)
                    continue;

                // don't expand the last node
                if (i > 0)
                    container.IsExpanded = true;

                container.ApplyTemplate();
                var itemsPresenter = (ItemsPresenter)container.Template.FindName("ItemsHost", container);
                if (itemsPresenter == null)
                {
                    // The Tree template has not named the ItemsPresenter, 
                    // so walk the descendents and find the child.
                    itemsPresenter = container.FindVisualChildOfType<ItemsPresenter>();
                    if (itemsPresenter == null)
                    {
                        container.UpdateLayout();
                        itemsPresenter = container.FindVisualChildOfType<ItemsPresenter>();
                    }
                }
                if (itemsPresenter == null)
                    return false;

                itemsPresenter.ApplyTemplate();
                var itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);
                itemsHostPanel.UpdateLayout();
                itemsHostPanel.ApplyTemplate();

                // Ensure that the generator for this panel has been created.
                // ReSharper disable once UnusedVariable
                var children = itemsHostPanel.Children;
                container.BringIntoView();
            }
            return true;
        }

        internal virtual void SelectFromUiAutomation([NotNull] TreeViewItem item)
        {
            SelectSingleItem(item);
            item.ForceFocus();
        }

        internal virtual void SelectParentFromKey()
        {
            var item = GetFocusedItem()?.ParentTreeViewItem;
            if (item == null) return;
            
            // if ctrl is pressed just focus it, so it can be selected by space. Otherwise select it.
            if (!IsControlKeyDown)
            {
                SelectSingleItem(item);
            }

            item.ForceFocus();
        }

        internal virtual void SelectPreviousFromKey()
        {
            var items = TreeViewElementFinder.FindAll(this, true).ToList();
            var item = GetFocusedItem();
            if (item == null) return;
            item = GetPreviousItem(item, items);
            if (item == null) return;

            // if ctrl is pressed just focus it, so it can be selected by space. Otherwise select it.
            if (!IsControlKeyDown)
            {
                SelectSingleItem(item);
            }

            item.ForceFocus();
        }

        internal virtual void SelectNextFromKey()
        {
            var item = GetFocusedItem();
            item = TreeViewElementFinder.FindNext(item, true);
            if (item == null) return;

            // if ctrl is pressed just focus it, so it can be selected by space. Otherwise select it.
            if (!IsControlKeyDown)
            {
                SelectSingleItem(item);
            }

            item.ForceFocus();
        }

        internal virtual void SelectCurrentBySpace()
        {
            var item = GetFocusedItem();
            if (item == null) return;
            SelectSingleItem(item);
            item.ForceFocus();
        }

        internal virtual void SelectFromProperty([NotNull] TreeViewItem item, bool isSelected)
        {
            // we do not check if selection is allowed, because selecting on that way is no user action.
            // Hopefully the programmer knows what he does...
            if (isSelected)
            {
                ModifySelection(new List<object>(1) { item.DataContext }, new List<object>());
                item.ForceFocus();
            }
            else
            {
                ModifySelection(new List<object>(), new List<object>(1) { item.DataContext });
            }
        }

        internal virtual void SelectFirst()
        {
            var item = TreeViewElementFinder.FindFirst(this, true);
            if (item == null)
                return;

            SelectSingleItem(item);
            item.ForceFocus();
        }

        internal virtual void SelectLast()
        {
            var item = TreeViewElementFinder.FindLast(this, true);
            if (item == null)
                return;

            SelectSingleItem(item);
            item.ForceFocus();
        }

        internal virtual void ClearObsoleteItems([NotNull] IList items)
        {
            updatingSelection = true;
            foreach (var itemToUnSelect in items)
            {
                SelectedItems.Remove(itemToUnSelect);
                if (SelectedItem == itemToUnSelect)
                    SelectedItem = null;
            }
            updatingSelection = false;

            if (SelectionMode != SelectionMode.Single && items.Contains(lastShiftRoot))
                lastShiftRoot = null;
        }

        /// <inheritdoc />
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            StopEditing();

            pointerPressed = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;

            var item = GetTreeViewItemUnderMouse(e.GetPosition(this));
            if (item == null) return;
            if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed || item.ContextMenu == null) return;
            if (item.IsEditing) return;

            if (!SelectedItems.Contains(item.DataContext))
            {
                SelectSingleItem(item);
            }

            item.ForceFocus();
        }

        /// <inheritdoc />
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (pointerPressed)
            {
                var item = GetTreeViewItemUnderMouse(e.GetPosition(this));
                if (item == null) return;
                if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
                if (item.IsEditing) return;

                SelectSingleItem(item);

                item.ForceFocus();
            }
            scrollViewerReentrency = false;
            pointerPressed = false;
        }

        private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (pointerPressed && !scrollViewerReentrency)
            {
                scrollViewerReentrency = true;
                scroller.ScrollToVerticalOffset(e.VerticalOffset - e.VerticalChange);
            }
        }

        internal void StartEditing([NotNull] TreeViewItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            StopEditing();
            editedItem = item;
        }

        internal void StopEditing()
        {
            if (stoppingEdition || editedItem == null)
                return;

            stoppingEdition = true;
            editedItem.Focus();
            editedItem.ForceFocus();
            editedItem = null;
            stoppingEdition = false;
        }

        internal IEnumerable<TreeViewItem> GetChildren(TreeViewItem item)
        {
            if (item == null) yield break;
            for (var i = 0; i < item.Items.Count; i++)
            {
                var child = item.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                if (child != null) yield return child;
            }
        }

        [CanBeNull]
        internal TreeViewItem GetNextItem([NotNull] TreeViewItem item, [ItemNotNull, NotNull]  List<TreeViewItem> items)
        {
            var indexOfCurrent = items.IndexOf(item);

            for (var i = indexOfCurrent + 1; i < items.Count; i++)
            {
                if (items[i].IsVisible)
                {
                    return items[i];
                }
            }

            return null;
        }

        [NotNull]
        internal IEnumerable<TreeViewItem> GetNodesToSelectBetween([NotNull] TreeViewItem firstNode, [NotNull] TreeViewItem lastNode)
        {
            var allNodes = TreeViewElementFinder.FindAll(this, false).ToList();
            var firstIndex = allNodes.IndexOf(firstNode);
            var lastIndex = allNodes.IndexOf(lastNode);

            if (firstIndex >= allNodes.Count)
            {
                throw new InvalidOperationException(
                   "First node index " + firstIndex + "greater or equal than count " + allNodes.Count + ".");
            }

            if (lastIndex >= allNodes.Count)
            {
                throw new InvalidOperationException(
                   "Last node index " + lastIndex + " greater or equal than count " + allNodes.Count + ".");
            }

            var nodesToSelect = new List<TreeViewItem>();

            if (lastIndex == firstIndex)
            {
                return new List<TreeViewItem> { firstNode };
            }

            if (lastIndex > firstIndex)
            {
                for (var i = firstIndex; i <= lastIndex; i++)
                {
                    if (allNodes[i].IsVisible)
                    {
                        nodesToSelect.Add(allNodes[i]);
                    }
                }
            }
            else
            {
                for (var i = firstIndex; i >= lastIndex; i--)
                {
                    if (allNodes[i].IsVisible)
                    {
                        nodesToSelect.Add(allNodes[i]);
                    }
                }
            }

            return nodesToSelect;
        }

        [CanBeNull]
        internal TreeViewItem GetPreviousItem([NotNull] TreeViewItem item, [ItemNotNull, NotNull]  List<TreeViewItem> items)
        {
            var indexOfCurrent = items.IndexOf(item);
            for (var i = indexOfCurrent - 1; i >= 0; i--)
            {
                if (items[i].IsVisible)
                {
                    return items[i];
                }
            }

            return null;
        }

        [CanBeNull]
        public TreeViewItem GetTreeViewItemFor(object item)
        {
            return TreeViewElementFinder.FindAll(this, false).FirstOrDefault(treeViewItem => item == treeViewItem.DataContext);
        }

        [ItemNotNull]
        internal IEnumerable<TreeViewItem> GetTreeViewItemsFor(IEnumerable objects)
        {
            if (objects == null)
            {
                yield break;
            }
            var items = objects.Cast<object>().ToList();
            foreach (var treeViewItem in TreeViewElementFinder.FindAll(this, false))
            {
                if (items.Contains(treeViewItem.DataContext))
                {
                    yield return treeViewItem;
                }
            }

        }

        private static void OnSelectedItemPropertyChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var treeView = (TreeView)d;
            if (treeView.updatingSelection)
                return;

            if (treeView.SelectedItems.Count == 1 && treeView.SelectedItems[0] == e.NewValue)
                return;

            treeView.updatingSelection = true;
            if (treeView.SelectedItems.Count > 0)
            {
                foreach (var oldItem in treeView.SelectedItems.Cast<object>().ToList())
                {
                    var item = treeView.GetTreeViewItemFor(oldItem);
                    if (item != null)
                        item.IsSelected = false;
                }
                treeView.SelectedItems.Clear();
            }
            if (e.NewValue != null)
            {
                var item = treeView.GetTreeViewItemFor(e.NewValue);
                if (item != null)
                {
                    // Setting to true will automatically add to the selection (if not already selected).
                    // See <see cref="TreeViewItem.OnPropertyChanged" /> for more details
                    item.IsSelected = true;
                }
                else
                {
                    treeView.SelectedItems.Add(e.NewValue);
                }
            }
            treeView.updatingSelection = false;
        }

        private static void OnSelectedItemsPropertyChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var treeView = (TreeView)d;
            if (e.OldValue != null)
            {
                var collection = e.OldValue as INotifyCollectionChanged;
                if (collection != null)
                {
                    collection.CollectionChanged -= treeView.OnSelectedItemsChanged;
                }
            }

            if (e.NewValue != null)
            {
                var collection = e.NewValue as INotifyCollectionChanged;
                if (collection != null)
                {
                    collection.CollectionChanged += treeView.OnSelectedItemsChanged;
                }
            }
        }

        private static void OnSelectionModeChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var newValue = (SelectionMode)e.NewValue;
            switch (newValue)
            {
                case SelectionMode.Single:
                    break;

                case SelectionMode.Multiple:
                    var treeView = (TreeView)d;
                    var selectedItem = treeView.SelectedItem;
                    treeView.updatingSelection = true;
                    for (var i = treeView.SelectedItems.Count - 1; i >= 0; --i)
                    {
                        if (treeView.SelectedItems[i] == selectedItem)
                            continue;

                        var item = treeView.GetTreeViewItemFor(treeView.SelectedItems[i]);
                        item?.IsSelected = false;
                        treeView.SelectedItems.RemoveAt(i);
                    }
                    treeView.updatingSelection = false;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (updatingSelection)
                return;

            if (SelectionMode == SelectionMode.Single && !allowedSelectionChanges)
                throw new InvalidOperationException("Can only change SelectedItems collection in multiple selection modes. Use SelectedItem in single select modes.");

            updatingSelection = true;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    object last = null;
                    foreach (var item in GetTreeViewItemsFor(e.NewItems))
                    {
                        item.IsSelected = true;

                        last = item.DataContext;
                    }

                    SelectedItem = last;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in GetTreeViewItemsFor(e.OldItems))
                    {
                        item.IsSelected = false;
                        if (item.DataContext == SelectedItem)
                        {
                            SelectedItem = SelectedItems.Count > 0 ? SelectedItems[SelectedItems.Count - 1] : null;
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var item in TreeViewElementFinder.FindAll(this, false))
                    {
                        if (item.IsSelected)
                        {
                            item.IsSelected = false;
                        }
                    }

                    SelectedItem = null;
                    break;
                default:
                    throw new InvalidOperationException();
            }
            updatingSelection = false;
        }

        /// <summary>
        ///     This method is invoked when the Items property changes.
        /// </summary>
        protected void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    ClearObsoleteItems(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Move:
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        protected void SelectSingleItem([NotNull] TreeViewItem item)
        {
            // selection with SHIFT is not working in virtualized mode. Thats because the Items are not visible.
            // Therefore the children cannot be found/selected.
            if (SelectionMode != SelectionMode.Single && IsShiftKeyDown && SelectedItems.Count > 0 && !IsVirtualizing)
            {
                SelectWithShift(item);
                return;
            }

            if (IsControlKeyDown)
            {
                ToggleItem(item);
            }
            else
            {
                ModifySelection(new List<object>(1) { item.DataContext }, new List<object>((IEnumerable<object>)SelectedItems));
            }
        }

        [CanBeNull]
        protected TreeViewItem GetFocusedItem()
        {
            return TreeViewElementFinder.FindAll(this, false).FirstOrDefault(x => x.IsFocused);
        }

        [CanBeNull]
        protected TreeViewItem GetTreeViewItemUnderMouse(Point positionRelativeToTree)
        {
            var visual = this.InputHitTest(positionRelativeToTree);
            if (visual == null)
                return null;
            
            var current = visual as Visual;
            while (current != null)
            {
                if (current is TreeViewItem tvi)
                {
                    return tvi.IsVisible ? tvi : null;
                }
        
                if (current is TreeView)
                {
                    return null;
                }
        
                current = current.GetVisualParent();
            }

            return null;
        }

        private void ToggleItem([NotNull] TreeViewItem item)
        {
            if (item.DataContext == null)
                return;

            var itemsToUnselect = SelectionMode == SelectionMode.Single ? new List<object>(SelectedItems.Cast<object>()) : new List<object>();
            if (SelectedItems.Contains(item.DataContext))
            {
                itemsToUnselect.Add(item.DataContext);
                ModifySelection(new List<object>(), itemsToUnselect);
            }
            else
            {
                ModifySelection(new List<object>(1) { item.DataContext }, itemsToUnselect);
            }
        }

        private void ModifySelection([NotNull] ICollection<object> itemsToSelect, [NotNull] ICollection<object> itemsToUnselect)
        {
            //clean up any duplicate or unnecessery input
            // check for itemsToUnselect also in itemsToSelect
            foreach (var item in itemsToSelect)
            {
                itemsToUnselect.Remove(item);
            }

            // check for itemsToSelect already in SelectedItems
            foreach (var item in SelectedItems)
            {
                itemsToSelect.Remove(item);
            }

            // check for itemsToUnSelect not in SelectedItems
            foreach (var item in itemsToUnselect.Where(x => !SelectedItems.Contains(x)).ToList())
            {
                itemsToUnselect.Remove(item);
            }

            //check if there's anything to do.
            if (itemsToSelect.Count == 0 && itemsToUnselect.Count == 0)
                return;

            allowedSelectionChanges = true;
            // Unselect and then select items
            foreach (var itemToUnSelect in itemsToUnselect)
            {
                SelectedItems.Remove(itemToUnSelect);
            }

            ((NonGenericObservableListWrapper<object>)SelectedItems).AddRange(itemsToSelect);
            allowedSelectionChanges = false;

            if (itemsToUnselect.Contains(lastShiftRoot))
                lastShiftRoot = null;

            if (!(SelectedItems.Contains(lastShiftRoot) && IsShiftKeyDown))
                lastShiftRoot = itemsToSelect.LastOrDefault();
        }
        
        private void SelectWithShift([NotNull] TreeViewItem item)
        {
            object firstSelectedItem;
            if (lastShiftRoot != null)
            {
                firstSelectedItem = lastShiftRoot;
            }
            else
            {
                firstSelectedItem = SelectedItems.Count > 0 ? SelectedItems[0] : null;
            }

            var shiftRootItem = GetTreeViewItemsFor(new List<object> { firstSelectedItem }).First();

            var itemsToSelect = GetNodesToSelectBetween(shiftRootItem, item).Select(x => x.DataContext).ToList();
            var itemsToUnSelect = ((IEnumerable<object>)SelectedItems).ToList();

            ModifySelection(itemsToSelect, itemsToUnSelect);
        }
    }
}
