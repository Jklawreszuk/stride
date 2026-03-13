// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using Stride.Core.Annotations;

namespace Stride.Core.Presentation.Behaviors
{
    /// <summary>
    /// This behavior will ensure that the associated toggle button can be toggled only when both mouse down and mouse up
    /// events are received, preventing to toggle it when a popup window is open. This behavior is useful when binding the
    /// <see cref="Popup.IsOpen"/> property of a popup to the <see cref="ToggleButton.IsChecked"/> of a toggle button.
    /// </summary>
    public class ToggleButtonPopupBehavior : Behavior<ToggleButton>
    {
        private bool mouseDownOccurred;

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AddHandler(InputElement.PointerPressedEvent, MouseDown, RoutingStrategies.Tunnel);
            AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, MouseUp, RoutingStrategies.Tunnel);
        }

        private void MouseUp(object sender, [NotNull] PointerEventArgs e)
        {
            if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
            {
                mouseDownOccurred = true;
            }
        }

        private void MouseDown(object sender, PointerEventArgs e)
        {
            if (!mouseDownOccurred)
            {
                e.Pointer.Capture(null);
            }

            mouseDownOccurred = false;
        }
    }
}
