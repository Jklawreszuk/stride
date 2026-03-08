// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Internal;

namespace Stride.Core.Presentation.Behaviors
{
    public class OverrideCursorBehavior : Behavior<Control>
    {
        public static readonly AvaloniaProperty CursorProperty = AvaloniaProperty.Register<OverrideCursorBehavior, Cursor>("Cursor");

        public static readonly AvaloniaProperty ForceCursorProperty = AvaloniaProperty.Register<OverrideCursorBehavior, bool>("ForceCursor");

        public static readonly AvaloniaProperty IsActiveProperty = AvaloniaProperty.Register<OverrideCursorBehavior, bool>("IsActive", true);

        public Cursor Cursor { get { return (Cursor)GetValue(CursorProperty); } set { SetValue(CursorProperty, value); } }

        public bool IsActive { get { return (bool)GetValue(IsActiveProperty); } set { SetValue(IsActiveProperty, value.Box()); } }

        static OverrideCursorBehavior()
        {
            CursorProperty.Changed.AddClassHandler<AvaloniaObject>(PropertyChanged);
        }
        
        protected override void OnAttached()
        {
            base.OnAttached();
            UpdateCursorOverride();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Cursor = null;
            base.OnDetaching();
        }
        private static void PropertyChanged([NotNull] AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var behavior = (OverrideCursorBehavior)d;
            behavior.UpdateCursorOverride();
        }

        private void UpdateCursorOverride()
        {
            AssociatedObject?.Cursor = IsActive ? Cursor : null;
        }
    }
}
