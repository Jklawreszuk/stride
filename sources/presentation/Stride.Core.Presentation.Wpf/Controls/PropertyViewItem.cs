// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Collections;
using Stride.Core.Presentation.Extensions;
using Stride.Core.Presentation.Internal;

namespace Stride.Core.Presentation.Controls
{
    /// <summary>
    /// This class represents an item container of the <see cref="PropertyView"/> control.
    /// </summary>
    public class PropertyViewItem : ExpandableItemsControl
    {
        private readonly ObservableList<PropertyViewItem> properties = [];

        /// <summary>
        /// Identifies the <see cref="Highlightable"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty HighlightableProperty = AvaloniaProperty.Register<PropertyViewItem, bool>("Highlightable", true);

        /// <summary>
        /// Identifies the <see cref="IsHighlighted"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty IsHighlightedPropertyKey = AvaloniaProperty.RegisterDirect<PropertyViewItem, bool>("IsHighlighted", o => o.IsHighlighted);

        /// <summary>
        /// Identifies the <see cref="IsHovered"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty IsHoveredPropertyKey = AvaloniaProperty.RegisterDirect<PropertyViewItem, bool>("IsHovered", o => o.IsHovered);

        /// <summary>
        /// Identifies the <see cref="IsKeyboardActive"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty IsKeyboardActivePropertyKey = AvaloniaProperty.RegisterDirect<PropertyViewItem, bool>("IsKeyboardActive", o => o.IsKeyboardActive);

        /// <summary>
        /// Identifies the <see cref="Offset"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty OffsetPropertyKey = AvaloniaProperty.RegisterDirect<PropertyViewItem, double>("Offset", o => o.Offset);

        /// <summary>
        /// Identifies the <see cref="Increment"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty IncrementProperty = AvaloniaProperty.Register<PropertyViewItem, double>("Increment");

        static PropertyViewItem()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyViewItem"/> class.
        /// </summary>
        /// <param name="propertyView">The <see cref="PropertyView"/> instance in which this <see cref="PropertyViewItem"/> is contained.</param>
        public PropertyViewItem([NotNull] PropertyView propertyView)
        {
            if (propertyView == null) throw new ArgumentNullException(nameof(propertyView));
            PropertyView = propertyView;
            AddHandler(PointerMovedEvent, propertyView.ItemMouseMove, RoutingStrategies.Tunnel);
            PointerPressed += OnMouseLeftButtonDown;
            IsKeyboardFocusWithinProperty.Changed.AddClassHandler<AvaloniaObject>(propertyView.OnIsKeyboardFocusWithinChanged);
        }

        /// <summary>
        /// Gets the <see cref="PropertyView"/> control containing this instance of <see cref="PropertyViewItem"/>.
        /// </summary>
        public PropertyView PropertyView { get; }

        /// <summary>
        /// Gets the collection of <see cref="PropertyViewItem"/> instance contained in this control.
        /// </summary>
        public IReadOnlyCollection<PropertyViewItem> Properties => properties;

        /// <summary>
        /// Gets or sets whether this control can be highlighted.
        /// </summary>
        /// <seealso cref="IsHighlighted"/>
        public bool Highlightable { get { return (bool)GetValue(HighlightableProperty); } set { SetValue(HighlightableProperty, value.Box()); } }

        /// <summary>
        /// Gets whether this control is highlighted. The control is highlighted when <see cref="IsHovered"/> and <see cref="Highlightable"/> are both <c>true</c>
        /// </summary>
        /// <seealso cref="Highlightable"/>
        /// <seealso cref="IsHovered"/>
        public bool IsHighlighted => (bool)GetValue(IsHighlightedPropertyKey);

        /// <summary>
        /// Gets whether the mouse cursor is currently over this control.
        /// </summary>
        public bool IsHovered => (bool)GetValue(IsHoveredPropertyKey);

        /// <summary>
        /// Gets whether this control is the closest control to the control that has the keyboard focus.
        /// </summary>
        public bool IsKeyboardActive => (bool)GetValue(IsKeyboardActivePropertyKey);

        /// <summary>
        /// Gets the absolute offset of this <see cref="PropertyViewItem"/>.
        /// </summary>
        public double Offset { get { return (double)GetValue(OffsetPropertyKey); } private set { SetValue(OffsetPropertyKey, value); } }

        /// <summary>
        /// Gets or set the increment value used to calculate the <see cref="Offset "/>of the <see cref="PropertyViewItem"/> contained in the <see cref="Properties"/> of this control..
        /// </summary>
        public double Increment { get { return (double)GetValue(IncrementProperty); } set { SetValue(IncrementProperty, value); } }


        protected void OnMouseLeftButtonDown(object sender, PointerEventArgs e)
        {
            // base method can handle this event, but we still want to focus on it in this case.
            var handled = e.Handled;
            if (!handled && IsEnabled)
            {
                Focus();
                e.Handled = true;
            }
        }

        // TODO
        //protected override AutomationPeer OnCreateAutomationPeer()
        //{
        //    return (AutomationPeer)new TreeViewItemAutomationPeer(this);
        //}

        private static void OnIncrementChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var item = (PropertyViewItem)d;
            var delta = (double)e.NewValue - (double)e.OldValue;
            var subItems = item.FindVisualChildrenOfType<PropertyViewItem>();
            foreach (var subItem in subItems)
            {
                subItem.Offset += delta;
            }
        }
    }
}
