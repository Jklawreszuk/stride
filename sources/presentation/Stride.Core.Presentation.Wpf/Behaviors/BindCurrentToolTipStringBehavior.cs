// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace Stride.Core.Presentation.Behaviors
{
    /// <summary>
    /// Allows the bind the <see cref="ToolTip"/> property of a control to a particular target property when the attached control is hovered by the mouse.
    /// This behavior can be used to display the same message that the tool-tip in a status bar, for example.
    /// </summary>
    /// <remarks>This behavior can be used to display the tool tip of some controls in another place, such as a status bar.</remarks>
    public class BindCurrentToolTipStringBehavior : Behavior<Control>
    {
        /// <summary>
        /// Identifies the <see cref="ToolTipTarget"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ToolTipTargetProperty = AvaloniaProperty.Register<BindCurrentToolTipStringBehavior, string?>(nameof(ToolTipTarget), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        /// <summary>
        /// Identifies the <see cref="DefaultValue"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<string?> DefaultValueProperty = AvaloniaProperty.Register<BindCurrentToolTipStringBehavior, string?>(nameof(DefaultValue), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        /// <summary>
        /// Gets or sets the tool tip text of the control when the mouse is over the control, or <see cref="DefaultValue"/> otherwise. This property should usually be bound.
        /// </summary>
        public string ToolTipTarget { get { return (string)GetValue(ToolTipTargetProperty); } set { SetValue(ToolTipTargetProperty, value); } }

        /// <summary>
        /// Gets or sets the default value to set when the mouse is not over the control.
        /// </summary>
        public string DefaultValue { get { return (string)GetValue(DefaultValueProperty); } set { SetValue(DefaultValueProperty, value); } }
        
        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PointerEntered += PointerEntered;
            AssociatedObject.PointerExited += PointerExited;
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            AssociatedObject.PointerEntered -= PointerEntered;
            AssociatedObject.PointerExited -= PointerExited;
            base.OnDetaching();
        }

        private void PointerEntered(object sender, PointerEventArgs e)
        {
            var tooltip = ToolTip.GetTip(AssociatedObject)?.ToString();
            SetCurrentValue(ToolTipTargetProperty, tooltip);
        }

        private void PointerExited(object sender, PointerEventArgs e)
        {
            SetCurrentValue(ToolTipTargetProperty, DefaultValue);
        }

    }
}
