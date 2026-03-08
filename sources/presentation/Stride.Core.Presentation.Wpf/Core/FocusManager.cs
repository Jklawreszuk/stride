// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Avalonia;
using Avalonia.Controls;
using Stride.Core.Annotations;
using Avalonia.Reactive;

namespace Stride.Core.Presentation.Core
{
    /// <summary>
    /// This class hold the <see cref="IsFocusedProperty"/> attached dependency property that allows to give the focus to a control using bindings.
    /// </summary>
    public class FocusManager
    {
        /// <summary>
        /// Identify the IsFocused attached dependency property.
        /// </summary>
        public static readonly AttachedProperty<bool> IsFocusedProperty =
            AvaloniaProperty.RegisterAttached<FocusManager, Control, bool>("IsFocused", defaultValue: false);
        
        static FocusManager()
        {
            IsFocusedProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<bool>>(OnIsFocusedPropertyChanged));
        }
        
        /// <summary>
        /// Gets whether the given object has currently the focus.
        /// </summary>
        /// <param name="obj">The object. If it is not an <see cref="Control"/>, this method will return <c>false</c>.</param>
        /// <returns><c>true</c> if the given object has the focus, false otherwise.</returns>
        public static bool GetIsFocused(Control control)
        {
            return control is { IsFocused: true };
        }

        /// <summary>
        /// Gives the focus to the given object.
        /// </summary>
        /// <param name="obj">The object that should get the focus.</param>
        /// <param name="value">The state of the focus. If value is <c>true</c>, the object will get the focus. Otherwise, this method does nothing.</param>
        public static void SetIsFocused([NotNull] Control obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        /// <summary>
        /// Raised when the <see cref="IsFocusedProperty"/> dependency property is modified.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnIsFocusedPropertyChanged(AvaloniaPropertyChangedEventArgs<bool> e)
        {
            if (e.Sender is Control control && e.NewValue == true)
            {
                control.Focus();
            }
        }
    }
}
