// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Controls;

namespace Stride.Core.Presentation.Behaviors
{
    public class TilePanelNavigationBehavior : DeferredBehaviorBase<VirtualizingTilePanel>
    {
        private SelectingItemsControl selector;

        protected override void OnAttachedAndLoaded()
        {
            AvaloniaObject parent = AssociatedObject;
            while (parent != null)
            {
                parent = ((Visual)parent).GetVisualParent();
                if (parent is SelectingItemsControl)
                    break;
            }

            if (parent == null)
            {
                throw new InvalidOperationException("Unable to find a parent SelectingItemsControl to the associated VirtualizingTilePanel.");
            }

            selector = (SelectingItemsControl)parent;

            KeyboardNavigation.SetTabNavigation(selector, KeyboardNavigationMode.None);
            selector.AddHandler(InputElement.KeyDownEvent, OnAssociatedObjectKeyDown, RoutingStrategies.Tunnel);
        }

        protected override void OnDetachingAndUnloaded()
        {
            selector?.RemoveHandler(InputElement.KeyDownEvent, OnAssociatedObjectKeyDown);
            selector = null;
        }

        private void OnAssociatedObjectKeyDown(object sender, [NotNull] KeyEventArgs e)
        {
            if (selector.SelectedIndex == -1 || selector.Items.Count == 0)
                return;

            var topLevel = TopLevel.GetTopLevel(selector);
            if (topLevel is not Window)
                return;

            // Find the currently focused element (logical focus)
            var focusedElement = topLevel.FocusManager?.GetFocusedElement() as AvaloniaObject;
            if (focusedElement == null)
                return;

            // Because of virtualization, we have to find the container that correspond to the selected item (by its index)
            var element = selector.ItemContainerGenerator.ContainerFromIndex(selector.SelectedIndex);
            // focusedElement can be either the current selector or one of its item container
            if (!ReferenceEquals(focusedElement, selector) && !ReferenceEquals(focusedElement, element))
            {
                // In this case, it means another control is focused, either a control out of scope, or an editing text box in the scope of the item container
                return;
            }

            bool moved = false;

            switch (e.Key)
            {
                case Key.Right:
                    moved = AssociatedObject.Orientation == Orientation.Vertical ? MoveToNextItem() : MoveToNextLineItem(1);
                    break;

                case Key.Left:
                    moved = AssociatedObject.Orientation == Orientation.Vertical ? MoveToPreviousItem() : MoveToPreviousLineItem(1);
                    break;

                case Key.Up:
                    moved = AssociatedObject.Orientation == Orientation.Vertical ? MoveToPreviousLineItem(1) : MoveToPreviousItem();
                    break;

                case Key.Down:
                    moved = AssociatedObject.Orientation == Orientation.Vertical ? MoveToNextLineItem(1) : MoveToNextItem();
                    break;

                case Key.PageUp:
                    if (AssociatedObject.Orientation == Orientation.Vertical)
                    {
                        var itemHeight = AssociatedObject.ItemSlotSize.Height + AssociatedObject.MinimumItemSpacing * 2.0f;
                        moved = MoveToPreviousLineItem((int)(AssociatedObject.ViewportHeight / itemHeight));
                    }
                    else
                    {
                        var itemWidth = AssociatedObject.ItemSlotSize.Width + AssociatedObject.MinimumItemSpacing * 2.0f;
                        moved = MoveToPreviousLineItem((int)(AssociatedObject.ViewportWidth / itemWidth));
                    }
                    break;

                case Key.PageDown:
                    if (AssociatedObject.Orientation == Orientation.Vertical)
                    {
                        var itemHeight = AssociatedObject.ItemSlotSize.Height + AssociatedObject.MinimumItemSpacing * 2.0f;
                        moved = MoveToNextLineItem((int)(AssociatedObject.ViewportHeight / itemHeight));
                    }
                    else
                    {
                        var itemWidth = AssociatedObject.ItemSlotSize.Width + AssociatedObject.MinimumItemSpacing * 2.0f;
                        moved = MoveToNextLineItem((int)(AssociatedObject.ViewportWidth / itemWidth));
                    }
                    break;

                case Key.Home:
                    if (selector.ItemCount > 0)
                    {
                        selector.SelectedIndex = 0;
                        selector.ScrollIntoView(selector.SelectedItem);
                        moved = true;
                    }
                    break;

                case Key.End:
                    if (selector.ItemCount > 0)
                    {
                        selector.SelectedIndex = selector.ItemCount - 1;
                        selector.ScrollIntoView(selector.SelectedItem);
                        moved = true;
                    }
                    break;

                default:
                    return;
            }

            e.Handled = true;

            if (moved)
            {
                if (selector.SelectedIndex > -1)
                {
                    AssociatedObject.ScrollToIndexedItem(selector.SelectedIndex);

                    if (selector.ItemContainerGenerator != null && selector.SelectedItem != null)
                    {
                        var lbi = selector.ContainerFromItem(selector.SelectedItem);
                        lbi?.Focus();
                    }
                }
            }
        }

        private bool MoveToPreviousLineItem(int lineCount)
        {
            var moved = false;

            int newPos = selector.SelectedIndex - (AssociatedObject.ItemsPerLine * lineCount);

            if (newPos >= 0 && newPos < selector.ItemCount)
            {
                selector.SelectedIndex = newPos;
                moved = true;
            }

            return moved;
        }

        private bool MoveToNextLineItem(int lineCount)
        {
            var moved = false;

            if (AssociatedObject.ItemCount > -1)
            {
                int newPos = selector.SelectedIndex + (AssociatedObject.ItemsPerLine * lineCount);

                if (newPos < AssociatedObject.ItemCount && newPos >= 0 && newPos < selector.ItemCount)
                {
                    selector.SelectedIndex = newPos;
                    moved = true;
                }
            }
            return moved;
        }

        private bool MoveToPreviousItem()
        {
            bool moved = false;

            if (selector.SelectedIndex > 0)
            {
                selector.SelectedIndex--;
                moved = true;
            }
            if (!moved && selector.SelectedItem == null && selector.ItemCount > 0)
            {
                selector.SelectedIndex = 0;
                selector.ScrollIntoView(selector.SelectedItem);
                moved = true;
            }
            return moved;
        }

        private bool MoveToNextItem()
        {
            bool moved = false;

            if (selector.SelectedIndex < selector.ItemCount - 1)
            {
                selector.SelectedIndex++;
                moved = true;
            }
            if (!moved && selector.SelectedItem == null && selector.ItemCount > 0)
            {
                selector.SelectedIndex = selector.ItemCount - 1;
                selector.ScrollIntoView(selector.SelectedItem);
                moved = true;
            }
            return moved;
        }
    }
}
