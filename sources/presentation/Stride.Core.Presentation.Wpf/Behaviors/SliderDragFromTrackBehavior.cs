// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Extensions;

namespace Stride.Core.Presentation.Behaviors
{
    public class SliderDragFromTrackBehavior : Behavior<Slider>
    {
        private bool trackMouseDown;
        private Track track;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PointerPressed += OnPointerPressed;
            AssociatedObject.PointerReleased += OnPointerReleased;
            AssociatedObject.Initialized += SliderInitialized;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Initialized -= SliderInitialized;
            AssociatedObject.PointerPressed -= OnPointerPressed;
            AssociatedObject.PointerReleased -= OnPointerReleased;
            if (track != null && track.Thumb != null)
            {
                track.Thumb.PointerEntered -= PointerEntered;
            }
            base.OnDetaching();
        }

        private void SliderInitialized(object sender, EventArgs e)
        {
            AssociatedObject.ApplyTemplate();

            track = AssociatedObject.FindVisualChildOfType<Track>();
            if (track == null || track.Name != "PART_Track")
                throw new InvalidOperationException("The associated slider must have a Track child named 'PART_Track'");
            track.Thumb.PointerEntered += PointerEntered;
            AssociatedObject.Initialized += SliderInitialized;
        }

        private void OnPointerReleased(object sender, [NotNull] PointerReleasedEventArgs e)
        {
            if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
            {
                trackMouseDown = false;

                // Release pointer capture
                if (e.Pointer.Captured == AssociatedObject)
                    e.Pointer.Capture(null);
            }
        }
        private void OnPointerPressed(object sender, [NotNull] PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
            {
                trackMouseDown = true;
                e.Pointer.Capture(AssociatedObject);
            }
        }

        private void PointerEntered(object sender, [NotNull] PointerEventArgs e)
        {
            if (trackMouseDown)
            {
                //todo
                //var args = new PointerEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left) { RoutedEvent = Control.MouseLeftButtonDownEvent };
                //track.Thumb.RaiseEvent(args);
                trackMouseDown = false;
            }
        }
    }
}
