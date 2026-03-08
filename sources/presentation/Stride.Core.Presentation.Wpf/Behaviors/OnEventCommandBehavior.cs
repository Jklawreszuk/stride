// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System.Windows;
using System.Windows.Input;
using Avalonia;

namespace Stride.Core.Presentation.Behaviors
{
    /// <summary>
    /// An implementation of the <see cref="OnEventBehavior"/> class that allows to invoke a command when a specific event is raised.
    /// </summary>
    public class OnEventCommandBehavior : OnEventBehavior
    {
        /// <summary>
        /// Identifies the <see cref="Command"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<ICommand> CommandProperty = AvaloniaProperty.Register<OnEventCommandBehavior, ICommand>("Command");

        /// <summary>
        /// Identifies the <see cref="CommandParameter"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<object> CommandParameterProperty = AvaloniaProperty.Register<OnEventCommandBehavior, object>("CommandParameter");

        /// <summary>
        /// Gets or sets the command to invoke when the event is raised.
        /// </summary>
        public ICommand Command { get { return GetValue(CommandProperty); } set { SetValue(CommandProperty, value); } }

        /// <summary>
        /// Gets or sets the parameter of the command to invoke when the event is raised.
        /// </summary>
        public object CommandParameter { get { return GetValue(CommandParameterProperty); } set { SetValue(CommandParameterProperty, value); } }

        /// <inheritdoc/>
        protected override void OnEvent()
        {
            var cmd = Command;

            if (cmd != null && cmd.CanExecute(CommandParameter))
                cmd.Execute(CommandParameter);
        }
    }
}
