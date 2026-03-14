// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using Stride.Core.Presentation.Commands;
using Stride.Core.Presentation.Internal;

namespace Stride.Core.Presentation.Behaviors
{
    /// <summary>
    /// This command will bind a <see cref="ICommandBase"/> to a <see cref="RoutedCommand"/>. It works just as a <see cref="CommandBinding"/> except that the bound
    /// command is executed when the routed command is executed. The <b>CanExecute</b> handler also invoke the <b>CanExecute</b> method of the <see cref="ICommandBase"/>.
    /// </summary>
    [Obsolete("Remove that later")]
    public class CommandBindingBehavior : Behavior<Control>
    {

        /// <summary>
        /// Identifies the <see cref="Command"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty CommandProperty =
            AvaloniaProperty.Register<CommandBindingBehavior, ICommandBase>(nameof(Command));
        /// <summary>
        /// Identifies the <see cref="ContinueRouting"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ContinueRoutingProperty =
            AvaloniaProperty.Register<CommandBindingBehavior, bool>(nameof(ContinueRouting), true);
        /// <summary>
        /// Identifies the <see cref="IsEnabled"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty IsEnabledProperty =
            AvaloniaProperty.Register<CommandBindingBehavior, bool>(nameof(IsEnabled), true);
        /// <summary>
        /// Identifies the <see cref="RoutedCommand"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ICommandProperty =
            AvaloniaProperty.Register<CommandBindingBehavior, ICommand>(nameof(ICommand));

        /// <summary>
        /// Gets or sets the <see cref="ICommandBase"/> to bind.
        /// </summary>
        public ICommandBase Command { get { return (ICommandBase)GetValue(CommandProperty); } set { SetValue(CommandProperty, value); } }

        /// <summary>
        /// Gets or sets whether the input routed event that invoked the command should continue to route through the element tree.
        /// </summary>
        /// <seealso cref="CanExecuteRoutedEventArgs.ContinueRouting"/>
        public bool ContinueRouting { get { return (bool)GetValue(ContinueRoutingProperty); } set { SetValue(ContinueRoutingProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets whether this command binding is enabled. When disabled, the <see cref="Command"/> won't be executed.
        /// </summary>
        public bool IsEnabled { get { return (bool)GetValue(IsEnabledProperty); } set { SetValue(IsEnabledProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets the <see cref="ICommand"/> to bind.
        /// </summary>
        public ICommand ICommand { get { return (ICommand)GetValue(ICommandProperty); } set { SetValue(ICommandProperty, value); } }

        static CommandBindingBehavior()
        {
            CommandProperty.Changed.AddClassHandler<AvaloniaObject>(CommandChanged);
        }
        
        /// <inheritdoc/>
        protected override void OnAttached()
        {
            
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            
        }

        private static void CommandChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            
        }

    }
}
