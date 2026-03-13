// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using Stride.Core.Presentation.Internal;
using NotNullAttribute = Stride.Core.Annotations.NotNullAttribute;

namespace Stride.Core.Presentation.Behaviors
{
    /// <summary>
    /// Base class for behaviors that capture the mouse.
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    public abstract class MouseMoveCaptureBehaviorBase<TElement> : Behavior<TElement>
        where TElement : Control
    {
        /// <summary>
        /// Identifies the <see cref="IsEnabled"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty IsEnabledProperty =
            AvaloniaProperty.Register<MouseMoveCaptureBehaviorBase<TElement>, bool>(nameof(IsEnabled), true);

        /// <summary>
        /// Identifies the <see cref="IsInProgress"/> dependency property key.
        /// </summary>
        protected static readonly AvaloniaProperty IsInProgressPropertyKey =
            AvaloniaProperty.RegisterDirect<MouseMoveCaptureBehaviorBase<TElement>, bool>(nameof(IsInProgress));

        /// <summary>
        /// Identifies the <see cref="IsInProgress"/> dependency property.
        /// </summary>
        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        public static readonly AvaloniaProperty IsInProgressProperty = IsInProgressPropertyKey.AvaloniaProperty;

        /// <summary>
        /// Identifies the <see cref="Modifiers"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ModifiersProperty =
            AvaloniaProperty.Register<MouseMoveCaptureBehaviorBase<TElement>, KeyModifiers?>(nameof(Modifiers));

        /// <summary>
        /// Identifies the <see cref="UsePreviewEvents"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty UsePreviewEventsProperty =
            AvaloniaProperty.Register<MouseMoveCaptureBehaviorBase<TElement>, bool>(nameof(UsePreviewEvents));
        
        /// <summary>
        /// <c>true</c> if this behavior is enabled; otherwise, <c>false</c>.
        /// </summary>
        public bool IsEnabled { get { return (bool)GetValue(IsEnabledProperty); } set { SetValue(IsEnabledProperty, value.Box()); } }

        /// <summary>
        /// <c>true</c> if an operation is in progress; otherwise, <c>false</c>.
        /// </summary>
        public bool IsInProgress { get { return (bool)GetValue(IsInProgressProperty); } private set { SetValue(IsInProgressPropertyKey, value.Box()); } }

        public KeyModifiers? Modifiers { get { return (KeyModifiers?)GetValue(ModifiersProperty); } set { SetValue(ModifiersProperty, value); } }

        public bool UsePreviewEvents
        {
            get { return (bool)GetValue(UsePreviewEventsProperty); }
            set { SetValue(UsePreviewEventsProperty, value.Box()); }
        }

        private static void IsEnabledChanged([NotNull] AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var behavior = (MouseMoveCaptureBehaviorBase<TElement>)d;
            if ((bool)e.NewValue != true)
            {
                behavior.Cancel();
            }
        }

        private static void UsePreviewEventsChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var behavior = (MouseMoveCaptureBehaviorBase<TElement>)d;
            behavior.UnsubscribeFromMouseEvents((bool)e.OldValue);
            behavior.SubscribeToMouseEvents((bool)e.NewValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool AreModifiersValid()
        {
            return Modifiers == null || (Modifiers == KeyModifiers.None ? Keyboard.Modifiers == KeyModifiers.None : Keyboard.Modifiers.HasFlag(Modifiers));
        }

        protected void Cancel()
        {
            if (!IsInProgress)
                return;

            ReleaseMouseCapture();
            CancelOverride();
        }

        protected virtual void CancelOverride()
        {
            // Default implementation does nothing
        }

        /// <summary>
        /// Captures the mouse to the <see cref="Behavior{TElement}.AssociatedObject"/>.
        /// </summary>
        protected void CaptureMouse(PointerPressedEventArgs e)
        {
            AssociatedObject.Focus();
            e.Pointer.Capture(AssociatedObject);
            IsInProgress = true;
        }

        ///  <inheritdoc/>
        protected override void OnAttached()
        {
            SubscribeToMouseEvents(UsePreviewEvents);
            AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, MouseUp, handledEventsToo: true);
            AssociatedObject.PointerCaptureLost += OnLostMouseCapture;
        }

        ///  <inheritdoc/>
        protected override void OnDetaching()
        {
            UnsubscribeFromMouseEvents(UsePreviewEvents);
            AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, MouseUp);
            AssociatedObject.PointerCaptureLost -= OnLostMouseCapture;
        }

        protected abstract void OnMouseDown([NotNull] PointerPressedEventArgs e);

        protected abstract void OnMouseMove([NotNull] PointerEventArgs e);

        protected abstract void OnMouseUp([NotNull] PointerEventArgs e);

        /// <summary>
        /// Releases the mouse capture, if the <see cref="Behavior{TElement}.AssociatedObject"/> held the capture. 
        /// </summary>
        protected void ReleaseMouseCapture()
        {
            IsInProgress = false;
            if (AssociatedObject.IsMouseCaptured)
            {
                AssociatedObject.ReleaseMouseCapture();
            }
        }

        private void MouseDown(object sender, [NotNull] PointerPressedEventArgs e)
        {
            if (!IsEnabled || IsInProgress)
                return;

            OnMouseDown(e);
        }

        private void PointerMove(object sender, [NotNull] PointerEventArgs e)
        {
            if (!IsEnabled || !IsInProgress)
                return;

            OnMouseMove(e);
        }

        private void MouseUp(object sender, [NotNull] PointerEventArgs e)
        {
            if (!IsEnabled || !IsInProgress || !AssociatedObject.IsMouseCaptured)
                return;

            OnMouseUp(e);
        }

        private void OnLostMouseCapture(object sender, [NotNull] PointerCaptureLostEventArgs e)
        {
            if (!ReferenceEquals(Mouse.Captured, sender))
            {
                Cancel();
            }
        }

        private void SubscribeToMouseEvents(bool usePreviewEvents)
        {
            if (AssociatedObject == null)
                return;

            if (usePreviewEvents)
            {
                AssociatedObject.PreviewMouseDown += MouseDown;
                AssociatedObject.PreviewMouseMove += PointerMove;
            }
            else
            {
                AssociatedObject.MouseDown += MouseDown;
                AssociatedObject.PointerMoved += PointerMove;
            }
        }

        private void UnsubscribeFromMouseEvents(bool usePreviewEvents)
        {
            if (AssociatedObject == null)
                return;

            if (usePreviewEvents)
            {
                AssociatedObject.PreviewMouseDown -= MouseDown;
                AssociatedObject.PreviewMouseMove -= PointerMove;
            }
            else
            {
                AssociatedObject.MouseDown -= MouseDown;
                AssociatedObject.PointerMoved -= PointerMoved;
            }
        }
    }
}
