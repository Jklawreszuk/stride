// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System.Windows;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Internal;

namespace Stride.Core.Presentation.Behaviors
{
    public enum PointerEventType
    {
        None,
        PointerPressed,
        PointerReleased,
        PointerMoved,
        PreviewPointerPressed,
        PreviewPointerReleased,
        PreviewPointerMoved,
    }

    public class OnMouseEventBehavior : Behavior<Control>
    {
        public static readonly AvaloniaProperty EventTypeProperty = AvaloniaProperty.Register<OnMouseEventBehavior, PointerEventType>(nameof(EventType));

        /// <summary>
        /// Identifies the <see cref="Command"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty CommandProperty = AvaloniaProperty.Register<OnMouseEventBehavior, ICommand>(nameof(Command));

        /// <summary>
        /// Identifies the <see cref="HandleEvent"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty HandleEventProperty = AvaloniaProperty.Register<OnMouseEventBehavior, bool>(nameof(HandleEvent));

        /// <summary>
        /// Identifies the <see cref="Modifiers"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ModifiersProperty =
               AvaloniaProperty.Register<OnMouseEventBehavior, KeyModifiers?>(nameof(Modifiers));

        public PointerEventType EventType { get { return (PointerEventType)GetValue(EventTypeProperty); } set { SetValue(EventTypeProperty, value); } }

        /// <summary>
        /// Gets or sets the command to invoke when the event is raised.
        /// </summary>
        public ICommand Command { get { return (ICommand)GetValue(CommandProperty); } set { SetValue(CommandProperty, value); } }

        /// <summary>
        /// Gets or sets whether to set the event as handled.
        /// </summary>
        public bool HandleEvent { get { return (bool)GetValue(HandleEventProperty); } set { SetValue(HandleEventProperty, value.Box()); } }

        public KeyModifiers? Modifiers { get { return (KeyModifiers?)GetValue(ModifiersProperty); } set { SetValue(ModifiersProperty, value); } }

        static OnMouseEventBehavior()
        {
            EventTypeProperty.Changed.AddClassHandler<AvaloniaObject>(EventTypeChanged);
        }
        
        protected bool AreModifiersValid(KeyModifiers? modifiers)
        {
            if (modifiers == null)
                return true;
            return Modifiers == KeyModifiers.None ? modifiers == KeyModifiers.None : (modifiers & Modifiers.Value) == Modifiers.Value;;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            RegisterHandler(EventType);
        }

        protected override void OnDetaching()
        {
            UnregisterHandler(EventType);
            base.OnAttached();
        }

        private static void EventTypeChanged([NotNull] AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var behavior = (OnMouseEventBehavior)d;
            if (behavior.AssociatedObject == null)
                return;

            var oldValue = (PointerEventType)e.OldValue;
            behavior.UnregisterHandler(oldValue);
            var newValue = (PointerEventType)e.NewValue;
            behavior.RegisterHandler(newValue);
        }

        private void RegisterHandler(PointerEventType type)
        {
            switch (type)
            {
                case PointerEventType.PointerPressed:
                    AssociatedObject.PointerPressed += MouseButtonHandler;
                    break;
                case PointerEventType.PointerReleased:
                    AssociatedObject.PointerReleased += MouseButtonHandler;
                    break;
                case PointerEventType.PointerMoved:
                    AssociatedObject.PointerMoved += MouseMoveHandler;
                    break;
                case PointerEventType.PreviewPointerPressed:
                    AssociatedObject.AddHandler(InputElement.PointerPressedEvent, MouseButtonHandler, RoutingStrategies.Tunnel);
                    break;
                case PointerEventType.PreviewPointerReleased:
                    AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, MouseButtonHandler, RoutingStrategies.Tunnel);
                    break;
                case PointerEventType.PreviewPointerMoved:
                    AssociatedObject.AddHandler(InputElement.PointerMovedEvent, MouseButtonHandler, RoutingStrategies.Tunnel);
                    break;
            }
        }

        private void UnregisterHandler(PointerEventType type)
        {
            switch (type)
            {
                case PointerEventType.PointerPressed:
                    AssociatedObject.PointerPressed -= MouseButtonHandler;
                    break;
                case PointerEventType.PointerReleased:
                    AssociatedObject.PointerReleased -= MouseButtonHandler;
                    break;
                case PointerEventType.PreviewPointerPressed:
                    AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, MouseButtonHandler);
                    break;
                case PointerEventType.PreviewPointerReleased:
                    AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, MouseButtonHandler);
                    break;
                case PointerEventType.PreviewPointerMoved:
                    AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, MouseButtonHandler);
                    break;
            }
        }

        private void MouseButtonHandler(object sender, [NotNull] PointerEventArgs e)
        {
            if (!AreModifiersValid(e.KeyModifiers))
                return;

            MouseMoveHandler(sender, e);
        }

        private void MouseMoveHandler(object sender, [NotNull] PointerEventArgs e)
        {
            if (!AreModifiersValid(e.KeyModifiers))
                return;

            if (HandleEvent)
            {
                e.Handled = true;
            }
            var cmd = Command;
            var position = e.GetPosition(AssociatedObject);
            if (cmd != null && cmd.CanExecute(position))
                cmd.Execute(position);
        }
    }
}
