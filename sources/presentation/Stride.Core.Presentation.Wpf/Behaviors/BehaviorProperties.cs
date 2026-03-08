// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Reactive;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Extensions;

namespace Stride.Core.Presentation.Behaviors
{
    /// <summary>
    /// This static class contains attached dependency properties that can be used as behavior to add or change features of controls.
    /// </summary>
    public class BehaviorProperties
    {
        /// <summary>
        /// When attached to a <see cref="ScrollViewer"/> or a control that contains a <see cref="ScrollViewer"/>, this property allows to control whether the scroll viewer should handle scrolling with the mouse wheel.
        /// </summary>
        public static AvaloniaProperty HandlesMouseWheelScrollingProperty = AvaloniaProperty.RegisterAttached<BehaviorProperties, Control, bool>("HandlesMouseWheelScrolling");

        /// <summary>
        /// When attached to a <see cref="Window"/> that have the <see cref="Window.WindowStyle"/> value set to <see cref="WindowStyle.None"/>, prevent the window to expand over the taskbar when maximized.
        /// </summary>
        public static AvaloniaProperty KeepTaskbarWhenMaximizedProperty = AvaloniaProperty.RegisterAttached<BehaviorProperties, Control, bool>("KeepTaskbarWhenMaximized");

        static BehaviorProperties()
        {
            HandlesMouseWheelScrollingProperty.Changed.AddClassHandler<AvaloniaObject>(HandlesMouseWheelScrollingChanged);
            KeepTaskbarWhenMaximizedProperty.Changed.AddClassHandler<AvaloniaObject>(KeepTaskbarWhenMaximizedChanged);
        }

        /// <summary>
        /// Gets the current value of the <see cref="HandlesMouseWheelScrollingProperty"/> dependency property attached to the given <see cref="AvaloniaObject"/>.
        /// </summary>
        /// <param name="target">The target <see cref="AvaloniaObject"/>.</param>
        /// <returns>The value of the <see cref="HandlesMouseWheelScrollingProperty"/> dependency property.</returns>
        public static bool GetHandlesMouseWheelScrolling([NotNull] AvaloniaObject target)
        {
            return (bool)target.GetValue(HandlesMouseWheelScrollingProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="HandlesMouseWheelScrollingProperty"/> dependency property attached to the given <see cref="AvaloniaObject"/>.
        /// </summary>
        /// <param name="target">The target <see cref="AvaloniaObject"/>.</param>
        /// <param name="value">The value to set.</param>
        public static void SetHandlesMouseWheelScrolling([NotNull] AvaloniaObject target, bool value)
        {
            target.SetValue(HandlesMouseWheelScrollingProperty, value);
        }

        /// <summary>
        /// Gets the current value of the <see cref="KeepTaskbarWhenMaximizedProperty"/> dependency property attached to the given <see cref="AvaloniaObject"/>.
        /// </summary>
        /// <param name="target">The target <see cref="AvaloniaObject"/>.</param>
        /// <returns>The value of the <see cref="KeepTaskbarWhenMaximizedProperty"/> dependency property.</returns>
        public static bool GetKeepTaskbarWhenMaximized([NotNull] AvaloniaObject target)
        {
            return (bool)target.GetValue(KeepTaskbarWhenMaximizedProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="KeepTaskbarWhenMaximizedProperty"/> dependency property attached to the given <see cref="AvaloniaObject"/>.
        /// </summary>
        /// <param name="target">The target <see cref="AvaloniaObject"/>.</param>
        /// <param name="value">The value to set.</param>
        public static void SetKeepTaskbarWhenMaximized([NotNull] AvaloniaObject target, bool value)
        {
            target.SetValue(KeepTaskbarWhenMaximizedProperty, value);
        }

        private static void HandlesMouseWheelScrollingChanged([NotNull] AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var scrollViewer = d as ScrollViewer ?? d.FindVisualChildOfType<ScrollViewer>();

            if (scrollViewer != null)
            {
                // Yet another internal property that should be public.
                typeof(ScrollViewer).GetProperty("HandlesMouseWheelScrolling", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(scrollViewer, e.NewValue);
            }
            else
            {
                // The framework element is not loaded yet and thus the ScrollViewer is not reachable.
                if (d is Control frameworkElement && !frameworkElement.IsLoaded)
                {
                    // Let's delay the behavior till the scroll viewer is loaded.
                    frameworkElement.Loaded += (sender, args) =>
                    {
                        var dependencyObject = (AvaloniaObject)sender;
                        var loadedScrollViewer = dependencyObject.FindVisualChildOfType<ScrollViewer>();
                        if (loadedScrollViewer != null)
                            typeof(ScrollViewer).GetProperty("HandlesMouseWheelScrolling", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(loadedScrollViewer, e.NewValue);
                    };
                }
            }
        }

        private static void KeepTaskbarWhenMaximizedChanged([NotNull] AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            if (d is not Window window)
                return;

            void ApplyWorkArea()
            {
                var screen = window.Screens.ScreenFromWindow(window);
                if (screen == null)
                    return;

                var workArea = screen.WorkingArea;

                window.MaxWidth = workArea.Width;
                window.MaxHeight = workArea.Height;
            }

            window.Opened += (_, _) => ApplyWorkArea();

            window.GetObservable(Window.WindowStateProperty)
                .Subscribe(new AnonymousObserver<WindowState>(state =>
                {
                    if (state == WindowState.Maximized)
                        ApplyWorkArea();
                }));
        }
    }
}
