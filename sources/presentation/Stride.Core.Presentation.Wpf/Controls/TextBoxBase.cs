// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Reflection;
using System.Windows.Input;
using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using Stride.Core.Presentation.Core;
using Stride.Core.Presentation.Internal;
using CancelRoutedEventArgs = Stride.Core.Presentation.Core.CancelRoutedEventArgs;

namespace Stride.Core.Presentation.Controls
{
    /// <summary>
    /// An implementation of the <see cref="Avalonia.Controls.TextBox"/> control
    /// that provides additional features such as a proper validation/cancellation workflow.
    /// </summary>
    public class TextBoxBase : Avalonia.Controls.TextBox
    {
        private bool validating;
        private NavigationMethod lastNavMethod;

        /// <summary>
        /// Identifies the <see cref="HasText"/> dependency property.
        /// </summary>
        private static readonly AvaloniaProperty HasTextPropertyKey = AvaloniaProperty.RegisterDirect<TextBoxBase, bool>("HasText", o => o.HasText);

        /// <summary>
        /// Identifies the <see cref="GetFocusOnLoad"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty GetFocusOnLoadProperty = AvaloniaProperty.Register<TextBoxBase, bool>("GetFocusOnLoad");

        /// <summary>
        /// Identifies the <see cref="SelectAllOnFocus"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty SelectAllOnFocusProperty = AvaloniaProperty.Register<TextBoxBase, bool>("SelectAllOnFocus");

        /// <summary>
        /// Identifies the <see cref="WatermarkContent"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty WatermarkContentProperty = AvaloniaProperty.Register<TextBoxBase, object>("WatermarkContent");

        /// <summary>
        /// Identifies the <see cref="WatermarkContentTemplate"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty WatermarkContentTemplateProperty = AvaloniaProperty.Register<TextBoxBase, DataTemplate>("WatermarkContentTemplate");

        /// <summary>
        /// Identifies the <see cref="ValidateWithEnter"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ValidateWithEnterProperty = AvaloniaProperty.Register<TextBoxBase, bool>("ValidateWithEnter", true);

        /// <summary>
        /// Identifies the <see cref="ValidateOnTextChange"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ValidateOnTextChangeProperty = AvaloniaProperty.Register<TextBoxBase, bool>("ValidateOnTextChange");

        /// <summary>
        /// Identifies the <see cref="ValidateOnLostFocus"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ValidateOnLostFocusProperty = AvaloniaProperty.Register<TextBoxBase, bool>("ValidateOnLostFocus", true);

        /// <summary>
        /// Identifies the <see cref="CancelWithEscape"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty CancelWithEscapeProperty = AvaloniaProperty.Register<TextBoxBase, bool>("CancelWithEscape", true);

        /// <summary>
        /// Identifies the <see cref="CancelOnLostFocus"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty CancelOnLostFocusProperty = AvaloniaProperty.Register<TextBoxBase, bool>("CancelOnLostFocus");

        /// <summary>
        /// Identifies the <see cref="ValidateCommand"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ValidateCommandProperty = AvaloniaProperty.Register<TextBoxBase, ICommand>("ValidateCommand");

        /// <summary>
        /// Identifies the <see cref="ValidateCommandParameter"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ValidateCommandParameterProprty = AvaloniaProperty.Register<TextBoxBase, object>("ValidateCommandParameter");

        /// <summary>
        /// Identifies the <see cref="CancelCommand"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty CancelCommandProperty = AvaloniaProperty.Register<TextBoxBase, ICommand>("CancelCommand");

        /// <summary>
        /// Identifies the <see cref="CancelCommandParameter"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty CancelCommandParameterProprty = AvaloniaProperty.Register<TextBoxBase, object>("CancelCommandParameter");

        /// <summary>
        /// Raised just before the TextBox changes are validated. This event is cancellable
        /// </summary>
        public static readonly RoutedEvent ValidatingEvent = RoutedEvent.Register<TextBoxBase, RoutedEventArgs>("Validating", RoutingStrategies.Bubble);

        /// <summary>
        /// Raised when TextBox changes have been validated.
        /// </summary>
        public static readonly RoutedEvent ValidatedEvent = RoutedEvent.Register<TextBoxBase, RoutedEventArgs>("Validated", RoutingStrategies.Bubble);

        /// <summary>
        /// Raised when the TextBox changes are cancelled.
        /// </summary>
        public static readonly RoutedEvent CancelledEvent = RoutedEvent.Register<TextBoxBase, RoutedEventArgs>("Cancelled", RoutingStrategies.Bubble);

        /// <summary>
        /// Raised when TextBox Text to value binding fails during validation.
        /// </summary>
        public static readonly RoutedEvent TextToSourceValueConversionFailedEvent = RoutedEvent.Register<TextBoxBase, RoutedEventArgs>("TextBindingFailed", RoutingStrategies.Bubble);

        static TextBoxBase()
        {
            TextProperty.Changed.AddClassHandler<AvaloniaObject>(OnTextChanged);
        }

        public TextBoxBase()
        {
            Loaded += OnLoaded;
            AddHandler(PointerPressedEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
        }

        /// <summary>
        /// Gets whether this TextBox contains a non-empty text.
        /// </summary>
        public bool HasText { get { return (bool)GetValue(HasTextPropertyKey); } private set { SetValue(HasTextPropertyKey, value.Box()); } }

        /// <summary>
        /// Gets or sets whether the associated text box should get keyboard focus when this behavior is attached.
        /// </summary>
        public bool GetFocusOnLoad { get { return (bool)GetValue(GetFocusOnLoadProperty); } set { SetValue(GetFocusOnLoadProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets whether the text of the TextBox must be selected when the control gets focus.
        /// </summary>
        public bool SelectAllOnFocus { get { return (bool)GetValue(SelectAllOnFocusProperty); } set { SetValue(SelectAllOnFocusProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets the content to display when the TextBox is empty.
        /// </summary>
        public object WatermarkContent { get { return GetValue(WatermarkContentProperty); } set { SetValue(WatermarkContentProperty, value); } }

        /// <summary>
        /// Gets or sets the template of the content to display when the TextBox is empty.
        /// </summary>
        public DataTemplate WatermarkContentTemplate { get { return (DataTemplate)GetValue(WatermarkContentTemplateProperty); } set { SetValue(WatermarkContentTemplateProperty, value); } }

        /// <summary>
        /// Gets or sets whether the validation should happen when the user press <b>Enter</b>.
        /// </summary>
        public bool ValidateWithEnter { get { return (bool)GetValue(ValidateWithEnterProperty); } set { SetValue(ValidateWithEnterProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets whether the validation should happen as soon as the <see cref="TextBox.Text"/> is changed.
        /// </summary>
        public bool ValidateOnTextChange { get { return (bool)GetValue(ValidateOnTextChangeProperty); } set { SetValue(ValidateOnTextChangeProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets whether the validation should happen when the control losts focus.
        /// </summary>
        public bool ValidateOnLostFocus { get { return (bool)GetValue(ValidateOnLostFocusProperty); } set { SetValue(ValidateOnLostFocusProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets whether the cancellation should happen when the user press <b>Escape</b>.
        /// </summary>
        public bool CancelWithEscape { get { return (bool)GetValue(CancelWithEscapeProperty); } set { SetValue(CancelWithEscapeProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets whether the cancellation should happen when the control losts focus.
        /// </summary>
        public bool CancelOnLostFocus { get { return (bool)GetValue(CancelOnLostFocusProperty); } set { SetValue(CancelOnLostFocusProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets the command to execute when the validation occurs.
        /// </summary>
        public ICommand ValidateCommand { get { return (ICommand)GetValue(ValidateCommandProperty); } set { SetValue(ValidateCommandProperty, value); } }

        /// <summary>
        /// Gets or sets the parameter of the command to execute when the validation occurs.
        /// </summary>
        public object ValidateCommandParameter { get { return GetValue(ValidateCommandParameterProprty); } set { SetValue(ValidateCommandParameterProprty, value); } }

        /// <summary>
        /// Gets or sets the command to execute when the cancellation occurs.
        /// </summary>
        public ICommand CancelCommand { get { return (ICommand)GetValue(CancelCommandProperty); } set { SetValue(CancelCommandProperty, value); } }

        /// <summary>
        /// Gets or sets the parameter of the command to execute when the cancellation occurs.
        /// </summary>
        public object CancelCommandParameter { get { return GetValue(CancelCommandParameterProprty); } set { SetValue(CancelCommandParameterProprty, value); } }

        /// <summary>
        /// Raised just before the TextBox changes are validated. This event is cancellable
        /// </summary>
        public event CancelRoutedEventHandler Validating { add { AddHandler(ValidatingEvent, value); } remove { RemoveHandler(ValidatingEvent, value); } }

        /// <summary>
        /// Raised when TextBox changes have been validated.
        /// </summary>
        public event ValidationRoutedEventHandler<string> Validated { add { AddHandler(ValidatedEvent, value); } remove { RemoveHandler(ValidatedEvent, value); } }

        /// <summary>
        /// Raised when the TextBox changes are cancelled.
        /// </summary>
        public event EventHandler<RoutedEventArgs> Cancelled { add { AddHandler(CancelledEvent, value); } remove { RemoveHandler(CancelledEvent, value); } }

        /// <summary>
        /// Raised when TextBox Text to value binding fails during validation.
        /// </summary>
        public event EventHandler<RoutedEventArgs> TextToSourceValueConversionFailed { add { AddHandler(TextToSourceValueConversionFailedEvent, value); } remove { RemoveHandler(TextToSourceValueConversionFailedEvent, value); } }

        protected internal bool HasChangesToValidate { get; set; }

        /// <summary>
        /// Validates the current changes in the TextBox. Does nothing is there are no changes.
        /// </summary>
        public void Validate()
        {
            if (IsReadOnly || !HasChangesToValidate || validating)
                return;

            var cancelRoutedEventArgs = new CancelRoutedEventArgs(ValidatingEvent);
            OnValidating(cancelRoutedEventArgs);
            if (cancelRoutedEventArgs.Cancel)
                return;

            RaiseEvent(cancelRoutedEventArgs);
            if (cancelRoutedEventArgs.Cancel)
                return;

            if (!IsTextCompatibleWithValueBinding(Text))
            {
                var textBindingFailedArgs = new RoutedEventArgs(TextToSourceValueConversionFailedEvent);
                RaiseEvent(textBindingFailedArgs);
                // We allow this to continue through since it'll revert itself through later code.
            }

            validating = true;
            var coercedText = CoerceTextForValidation(Text);
            SetCurrentValue(TextProperty, coercedText);

            BindingExpressionBase expression = BindingOperations.GetBindingExpressionBase(this, TextProperty);
            try
            {
                expression?.UpdateSource();
            }
            catch (TargetInvocationException ex) when (ex.InnerException is InvalidCastException)
            {
                var textBindingFailedArgs = new RoutedEventArgs(TextToSourceValueConversionFailedEvent);
                RaiseEvent(textBindingFailedArgs);
            }

            ClearUndoStack();

            var validatedArgs = new ValidationRoutedEventArgs<string>(ValidatedEvent, coercedText);
            OnValidated();

            RaiseEvent(validatedArgs);
            if (ValidateCommand != null && ValidateCommand.CanExecute(ValidateCommandParameter))
                ValidateCommand.Execute(ValidateCommandParameter);
            validating = false;
            HasChangesToValidate = false;
        }

        /// <summary>
        /// Validates the content of the TextBox even if no changes occurred.
        /// </summary>
        public void ForceValidate()
        {
            HasChangesToValidate = true;
            Validate();
        }

        /// <summary>
        /// Cancels the current changes in the TextBox.
        /// </summary>
        public void Cancel()
        {
            if (IsReadOnly)
                return;

            BindingExpressionBase expression = BindingOperations.GetBindingExpressionBase(this, TextProperty);
            expression?.UpdateTarget();

            ClearUndoStack();

            var cancelledArgs = new RoutedEventArgs(CancelledEvent);
            OnCancelled();
            RaiseEvent(cancelledArgs);

            if (CancelCommand != null && CancelCommand.CanExecute(CancelCommandParameter))
                CancelCommand.Execute(CancelCommandParameter);
        }

        /// <summary>
        /// Raised when the text of the TextBox changes.
        /// </summary>
        /// <param name="oldValue">The old value of the <see cref="TextBox.Text"/> property.</param>
        /// <param name="newValue">The new value of the <see cref="TextBox.Text"/> property.</param>
        protected virtual void OnTextChanged(string oldValue, string newValue)
        {
        }

        /// <summary>
        /// Raised when the text of the TextBox is being validated.
        /// </summary>
        /// <param name="e">The event argument.</param>
        protected virtual void OnValidating(CancelRoutedEventArgs e)
        {
        }

        /// <summary>
        /// Raised when the current changes have has been validated.
        /// </summary>
        protected virtual void OnValidated()
        {
        }

        /// <summary>
        /// Raised when the current changes have been cancelled.
        /// </summary>
        protected virtual void OnCancelled()
        {
        }

        /// <summary>
        /// Preliminary check during validation to see if the text is in a valid format.
        /// </summary>
        protected virtual bool IsTextCompatibleWithValueBinding(string text)
        {
            return true;
        }

        /// <summary>
        /// Coerces the text during the validation process. This method is invoked by <see cref="Validate"/>.
        /// </summary>
        /// <param name="baseValue">The value to coerce.</param>
        /// <returns>The coerced value.</returns>
        protected virtual string CoerceTextForValidation(string baseValue)
        {
            return MaxLength > 0 && baseValue.Length > MaxLength ? baseValue.Substring(0, MaxLength) : baseValue;
        }

        /// <inheritdoc/>
        protected void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (IsReadOnly)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter && ValidateWithEnter)
            {
                Validate();
            }
            if (e.Key == Key.Escape && CancelWithEscape)
            {
                Cancel();
            }
        }

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            lastNavMethod = e.NavigationMethod;
            if (SelectAllOnFocus && e.NavigationMethod == NavigationMethod.Tab)
            {
                SelectAll();
            }

            base.OnGotFocus(e);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (!IsKeyboardFocusWithin)
            {
                if (SelectAllOnFocus)
                {
                    // We handle the event only when the SelectAllOnFocus property is active. If we don't handle it, base.OnMouseDown will clear the selection
                    // we're just about to do. But if we handle it, the caret won't be moved to the cursor position, which is the behavior we expect when SelectAllOnFocus is inactive.
                    e.Handled = true;
                }
                Focus();
            }
            base.OnPointerPressed(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (lastNavMethod != NavigationMethod.Tab)
            {
                return;
            }
            if (ValidateOnLostFocus && !validating)
            {
                Validate();
            }
            if (CancelOnLostFocus)
            {
                Cancel();
            }
            
            base.OnLostFocus(e);
        }

        private void ClearUndoStack()
        {
            var limit = UndoLimit;
            UndoLimit = 0;
            UndoLimit = limit;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (GetFocusOnLoad)
            {
                Focus();
            }
        }

        private static void OnTextChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var input = (TextBoxBase)d;
            input.HasText = e.NewValue != null && ((string)e.NewValue).Length > 0;
            if (!input.validating)
                input.HasChangesToValidate = true;

            input.OnTextChanged((string)e.OldValue, (string)e.NewValue);
            if (input.ValidateOnTextChange && !input.validating)
                input.Validate();
        }

        private static void OnLostFocusActionChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var input = (TextBoxBase)d;
            if (e.Property == ValidateOnLostFocusProperty && (bool)e.NewValue)
            {
                input.SetCurrentValue(CancelOnLostFocusProperty, false);
            }
            if (e.Property == CancelOnLostFocusProperty && (bool)e.NewValue)
            {
                input.SetCurrentValue(ValidateOnLostFocusProperty, false);
            }
        }
    }
}
