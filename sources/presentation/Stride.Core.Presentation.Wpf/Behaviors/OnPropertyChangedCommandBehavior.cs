// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Linq;
using System.Reflection;
using Stride.Core.Presentation.Core;
using Stride.Core.Presentation.Extensions;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Reactive;
using Avalonia.Xaml.Interactivity;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Internal;

namespace Stride.Core.Presentation.Behaviors
{
    /// <summary>
    /// A <see cref="Behavior{T}"/> that allow to execute a command when the value of a dependency property of its associated 
    /// object changes, or when the source of the dependency property binding is updated.
    /// </summary>
    public class OnPropertyChangedCommandBehavior : Behavior<Control>
    {
        private AvaloniaProperty dependencyProperty;
        private IDisposable subscription;

        /// <summary>
        /// Identifies the <see cref="Command"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty CommandProperty = AvaloniaProperty.Register<OnPropertyChangedCommandBehavior,ICommand>(nameof(Command));

        /// <summary>
        /// Identifies the <see cref="CommandParameter"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty CommandParameterProperty = AvaloniaProperty.Register<OnPropertyChangedCommandBehavior,object>(nameof(CommandParameter));
            
        /// <summary>
        /// Identifies the <see cref="ExecuteOnlyOnSourceUpdate"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ExecuteOnlyOnSourceUpdateProperty = AvaloniaProperty.Register<OnPropertyChangedCommandBehavior,bool>(nameof(ExecuteOnlyOnSourceUpdate));

        /// <summary>
        /// Identifies the <see cref="PassValueAsParameter"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty PassValueAsParameterProperty = AvaloniaProperty.Register<OnPropertyChangedCommandBehavior,bool>(nameof(PassValueAsParameter));
        
        /// <summary>
        /// Gets or sets the name of the dependency property that will trigger the associated command.
        /// </summary>
        /// <remarks>Changing this property after the behavior has been attached will have no effect.</remarks>
        public string PropertyName { get { return field; } set { if (AssociatedObject == null) field = value; } }

        /// <summary>
        /// Gets or sets the command to execute when the property is modified.
        /// </summary>
        public ICommand Command { get { return (ICommand)GetValue(CommandProperty); } set { SetValue(CommandProperty, value); } }

        /// <summary>
        /// Gets or sets the parameter of the command to execute when the property is modified.
        /// </summary>
        public object CommandParameter { get { return GetValue(CommandParameterProperty); } set { SetValue(CommandParameterProperty, value); } }

        /// <summary>
        /// Gets or set whether the command should be executed only when the source of the binding associated to the dependency property is updated.
        /// </summary>
        /// <remarks>If set to <c>true</c>, this property requires that a binding exists on the dependency property and that it has <see cref="Binding.NotifyOnSourceUpdated"/> set to <c>true</c>.</remarks>
        public bool ExecuteOnlyOnSourceUpdate { get { return (bool)GetValue(ExecuteOnlyOnSourceUpdateProperty); } set { SetValue(ExecuteOnlyOnSourceUpdateProperty, value.Box()); } }
        
        /// <summary>
        /// Gets or sets whether the value of the property should be used as the parameter of the command to execute when the property is modified.
        /// </summary>
        public bool PassValueAsParameter { get { return (bool)GetValue(PassValueAsParameterProperty); } set { SetValue(PassValueAsParameterProperty, value.Box()); } }

        protected override void OnAttached()
        {
            if (PropertyName == null)
                throw new ArgumentException($"PropertyName must be set.");

            dependencyProperty = AssociatedObject
                .GetType()
                .GetField(PropertyName + "Property",
                    BindingFlags.Static | BindingFlags.Public)
                ?.GetValue(null) as AvaloniaProperty;

            if (dependencyProperty == null)
                throw new ArgumentException($"Property '{PropertyName}' not found.");

            subscription = AssociatedObject
                .GetObservable(dependencyProperty)
                .Subscribe(new AnonymousObserver<object>(value => OnPropertyChanged(value)));
        }

        protected override void OnDetaching()
        {
            subscription?.Dispose();
            subscription = null;
            base.OnDetaching();
        }

        private void OnPropertyChanged(object value)
        {
            if (!ExecuteOnlyOnSourceUpdate)
            {
                ExecuteCommand(value);
            }
        }

        private void ExecuteCommand(object value)
        {
            var parameter = PassValueAsParameter ? value : CommandParameter;
            if (Command == null || !Command.CanExecute(parameter))
                return;

            Command.Execute(parameter);
        }
    }
}
