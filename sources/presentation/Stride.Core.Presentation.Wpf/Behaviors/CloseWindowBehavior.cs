// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Reflection;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Services;

namespace Stride.Core.Presentation.Behaviors
{
    /// <summary>
    /// A base behavior that will close the window it is contained in an event occurs on a control. A command can be executed
    /// before closing the window by using the <see cref="Command"/> and <see cref="CommandParameter"/> properties of this behavior.
    /// </summary>
    public abstract class CloseWindowBehavior<T> : Behavior<T> where T : AvaloniaObject
    {
        /// <summary>
        /// Identifies the <see cref="DialogResult"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty DialogResultProperty = AvaloniaProperty.Register<CloseWindowBehavior<T>, DialogResult>("DialogResult");

        /// <summary>
        /// Identifies the <see cref="Command"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty CommandProperty = AvaloniaProperty.Register<CloseWindowBehavior<T>, ICommand>("Command");

        /// <summary>
        /// Identifies the <see cref="CommandParameter"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty CommandParameterProperty = AvaloniaProperty.Register<CloseWindowBehavior<T>, object>("CommandParameter");

        /// <summary>
        /// Gets or sets the value to set to the <see cref="Services.DialogResult"/> property of the window the associated button is contained in.
        /// </summary>
        public DialogResult DialogResult { get { return (DialogResult)GetValue(DialogResultProperty); } set { SetValue(DialogResultProperty, value); } }

        /// <summary>
        /// Gets or sets the command to execute before closing the window.
        /// </summary>
        public ICommand Command { get { return (ICommand)GetValue(CommandProperty); } set { SetValue(CommandProperty, value); } }

        /// <summary>
        /// Gets or sets the parameter of the command to execute before closing the window.
        /// </summary>
        public object CommandParameter { get { return GetValue(CommandParameterProperty); } set { SetValue(CommandParameterProperty, value); } }

        static CloseWindowBehavior()
        {
            CommandProperty.Changed.AddClassHandler<CloseWindowBehavior<T>>((o, e) => CommandChanged(o, e));
            CommandParameterProperty.Changed.AddClassHandler<CloseWindowBehavior<T>>((o, e) => CommandChanged(o, e));
            
        }
        
        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();
            if (Command != null)
            {
                AssociatedObject.SetCurrentValue(Control.IsEnabledProperty, Command.CanExecute(CommandParameter));
            }
        }

        private static void CommandChanged([NotNull] AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var behavior = (ButtonCloseWindowBehavior)d;
            var newCommand = e.NewValue as ICommand;

            if (e.OldValue is ICommand oldCommand)
            {
                oldCommand.CanExecuteChanged -= behavior.CommandCanExecuteChanged;
            }
            if (newCommand != null)
            {
                newCommand.CanExecuteChanged += behavior.CommandCanExecuteChanged;
            }
        }

        private static void CommandParameterChanged([NotNull] AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var behavior = (ButtonCloseWindowBehavior)d;
            if (behavior.Command != null)
            {
                behavior.AssociatedObject.SetCurrentValue(Control.IsEnabledProperty, behavior.Command.CanExecute(behavior.CommandParameter));
            }
        }

        private void CommandCanExecuteChanged(object sender, EventArgs e)
        {
            AssociatedObject.SetCurrentValue(Control.IsEnabledProperty, Command.CanExecute(CommandParameter));
        }

        /// <summary>
        /// Invokes the command and close the containing window.
        /// </summary>
        protected void Close()
        {
            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }

            if (AssociatedObject is not Visual visualObject)
            {
                return;
            }

            if (TopLevel.GetTopLevel(visualObject) is not Window window)
            {
                throw new InvalidOperationException("The button attached to this behavior is not in a window");
            }

            if (window is IModalDialogInternal modal)
            {
                modal.Result = DialogResult; //todo Avalonia uses ShowDialog to get DialogResult 
            }

            window.Close();
        }
    }

    internal static class WpfModalHelper
    {
        private static readonly FieldInfo ShowingAsDialog;
        private const string FieldName = "_showingAsDialog";

        static WpfModalHelper()
        {
            ShowingAsDialog = typeof(Window).GetField(FieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (ShowingAsDialog == null)
                throw new MissingFieldException(nameof(Window), FieldName);
        }

        public static bool IsModal([NotNull] Window window)
        {
            return (bool)ShowingAsDialog.GetValue(window);
        }

        public static DialogResult ToDialogResult(bool? dialogResult)
        {
            if (dialogResult.HasValue)
            {
                return dialogResult.Value ? DialogResult.Ok : DialogResult.Cancel;
            }
            return DialogResult.None;
        }

        public static bool? ToDialogResult(DialogResult dialogResult)
        {
            return dialogResult switch
            {
                DialogResult.None => null,
                DialogResult.Ok => true,
                DialogResult.Cancel => false,
                _ => throw new ArgumentOutOfRangeException(nameof(dialogResult), dialogResult, null)
            };
        }
    }
}
