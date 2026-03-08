// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System.Windows;
using System.Windows.Input;
using Avalonia;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Controls;

namespace Stride.Core.Presentation.Behaviors
{
    public class TextBoxKeyUpCommandBehavior : Behavior<TextBoxBase>
    {
        /// <summary>
        /// Identifies the <see cref="Command"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<ICommand> CommandProperty = AvaloniaProperty.Register<TextBoxKeyUpCommandBehavior,ICommand>("Command");

        /// <summary>
        /// Identifies the <see cref="Key"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<Key> KeyProperty = AvaloniaProperty.Register<TextBoxKeyUpCommandBehavior,Key>("Key", Key.Enter);

        /// <summary>
        /// Gets or sets the command to invoke.
        /// </summary>
        public ICommand Command { get { return GetValue(CommandProperty); } set { SetValue(CommandProperty, value); } }

        /// <summary>
        /// Gets or sets the key that should trigger this behavior. The default is <see cref="Avalonia.Input.Key.Enter"/>.
        /// </summary>
        public Key Key { get { return (Key)GetValue(KeyProperty); } set { SetValue(KeyProperty, value); } }

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.KeyUp += KeyUp;
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            AssociatedObject.KeyUp -= KeyUp;
            base.OnDetaching();
        }

        private void KeyUp(object sender, [NotNull] KeyEventArgs e)
        {
            if (e.Key != Key || AssociatedObject.HasChangesToValidate)
            {
                return;
            }

            Command?.Execute(AssociatedObject.Text);
        }
    }
}
