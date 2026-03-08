// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Stride.Core.Presentation.Behaviors
{
    /// <summary>
    /// A behavior that can be attached to a <see cref="Control"/> and will close the window it is contained in when double-clicked.
    /// If you need to execute a command before closing the window, you can use the <see cref="CloseWindowBehavior{T}.Command"/> and <see cref="CloseWindowBehavior{T}.CommandParameter"/> property of this behavior.
    /// </summary>
    public class DoubleClickCloseWindowBehavior : CloseWindowBehavior<Control>
    {
        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PointerPressed += ControlDoubleClicked;
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            AssociatedObject.PointerPressed -= ControlDoubleClicked;
            base.OnDetaching();
        }

        /// <summary>
        /// Raised when the associated button is clicked. Close the containing window
        /// </summary>
        private void ControlDoubleClicked(object sender, PointerPressedEventArgs e)
        {
            if (e.ClickCount == 2)
                Close();
        }
    }
}
