// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Markup.Xaml.Templates;

namespace Stride.Core.Presentation.Controls
{
    public sealed class TimeSpanEditor : TemplatedControl
    {
        private bool interlock;
        private bool templateApplied;
        private AvaloniaProperty initializingProperty;

        /// <summary>
        /// Identifies the <see cref="WatermarkContent"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty WatermarkContentProperty = AvaloniaProperty.Register<TimeSpanEditor,  object>("WatermarkContent");

        /// <summary>
        /// Identifies the <see cref="WatermarkContentTemplate"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty WatermarkContentTemplateProperty = AvaloniaProperty.Register<TimeSpanEditor,  DataTemplate>("WatermarkContentTemplate");

        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ValueProperty = AvaloniaProperty.Register<TimeSpanEditor, TimeSpan?>("Value");

        /// <summary>
        /// Identifies the <see cref="Days"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty DaysProperty = AvaloniaProperty.Register<TimeSpanEditor, int?>("Days");

        /// <summary>
        /// Identifies the <see cref="Hours"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty HoursProperty = AvaloniaProperty.Register<TimeSpanEditor,  int?>("Hours");

        /// <summary>
        /// Identifies the <see cref="Minutes"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty MinutesProperty = AvaloniaProperty.Register<TimeSpanEditor,  int?>("Minutes");

        /// <summary>
        /// Identifies the <see cref="Seconds"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty SecondsProperty = AvaloniaProperty.Register<TimeSpanEditor,  double?>("Seconds");

        /// <summary>
        /// Gets or sets the content to display when the TextBox is empty.
        /// </summary>
        public object WatermarkContent { get { return GetValue(WatermarkContentProperty); } set { SetValue(WatermarkContentProperty, value); } }

        /// <summary>
        /// Gets or sets the template of the content to display when the TextBox is empty.
        /// </summary>
        public DataTemplate WatermarkContentTemplate { get { return (DataTemplate)GetValue(WatermarkContentTemplateProperty); } set { SetValue(WatermarkContentTemplateProperty, value); } }

        /// <summary>
        /// Gets or sets the time span associated to this control.
        /// </summary>
        public TimeSpan? Value { get { return (TimeSpan?)GetValue(ValueProperty); } set { SetValue(ValueProperty, value); } }

        /// <summary>
        /// Gets or sets the number of days displayed in the <see cref="TimeSpanEditor"/>.
        /// </summary>
        public int? Days { get { return (int?)GetValue(DaysProperty); } set { SetValue(DaysProperty, value); } }

        /// <summary>
        /// Gets or sets the number of hours displayed in the <see cref="TimeSpanEditor"/>.
        /// </summary>
        public int? Hours { get { return (int?)GetValue(HoursProperty); } set { SetValue(HoursProperty, value); } }

        /// <summary>
        /// Gets or sets the number of minutes displayed in the <see cref="TimeSpanEditor"/>.
        /// </summary>
        public int? Minutes { get { return (int?)GetValue(MinutesProperty); } set { SetValue(MinutesProperty, value); } }

        /// <summary>
        /// Gets or sets the number of seconds displayed in the <see cref="TimeSpanEditor"/>.
        /// </summary>
        public double? Seconds { get { return (double?)GetValue(SecondsProperty); } set { SetValue(SecondsProperty, value); } }

        /// <inheritdoc/>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            templateApplied = false;
            base.OnApplyTemplate(e);
            templateApplied = true;
        }

        /// <summary>
        /// Updates the properties corresponding to the components of the time span from the given time span value.
        /// </summary>
        /// <param name="value">The time span from which to update component properties.</param>
        private void UpdateComponentsFromValue(TimeSpan? value)
        {
            if (value != null)
            {
                SetCurrentValue(DaysProperty, value.Value.Days);
                SetCurrentValue(HoursProperty, value.Value.Hours);
                SetCurrentValue(MinutesProperty, value.Value.Minutes);
                SetCurrentValue(SecondsProperty, (double)(value.Value.Ticks % TimeSpan.TicksPerMinute) / TimeSpan.TicksPerSecond);
            }
        }

        /// <summary>
        /// Updates the <see cref="Value"/> property according to a change in the given component property.
        /// </summary>
        /// <param name="property">The component property from which to update the <see cref="Value"/>.</param>
        private TimeSpan? UpdateValueFromComponent(AvaloniaProperty property)
        {
            // NOTE: Precision must be on OS tick level.

            if (property == DaysProperty)
                return Days.HasValue && Value.HasValue ? (TimeSpan?)new TimeSpan(Days.Value * TimeSpan.TicksPerDay + Value.Value.Hours * TimeSpan.TicksPerHour + Value.Value.Minutes * TimeSpan.TicksPerMinute + Value.Value.Ticks % TimeSpan.TicksPerMinute) : null;
            if (property == HoursProperty)
                return Hours.HasValue && Value.HasValue ? (TimeSpan?)new TimeSpan(Value.Value.Days * TimeSpan.TicksPerDay + Hours.Value * TimeSpan.TicksPerHour + Value.Value.Minutes * TimeSpan.TicksPerMinute + Value.Value.Ticks % TimeSpan.TicksPerMinute) : null;
            if (property == MinutesProperty)
                return Minutes.HasValue && Value.HasValue ? (TimeSpan?)new TimeSpan(Value.Value.Days * TimeSpan.TicksPerDay + Value.Value.Hours * TimeSpan.TicksPerHour + Minutes.Value * TimeSpan.TicksPerMinute + Value.Value.Ticks % TimeSpan.TicksPerMinute) : null;
            if (property == SecondsProperty)
                return Seconds.HasValue && Value.HasValue ? (TimeSpan?)new TimeSpan(Value.Value.Days * TimeSpan.TicksPerDay + Value.Value.Hours * TimeSpan.TicksPerHour + Value.Value.Minutes * TimeSpan.TicksPerMinute + (long)(Seconds.Value * TimeSpan.TicksPerSecond)) : null;

            throw new ArgumentException("Property unsupported by method UpdateValueFromComponent.");
        }

        /// <summary>
        /// Raised when the <see cref="Value"/> property is modified.
        /// </summary>
        private void OnValueValueChanged()
        {
            var isInitializing = !templateApplied && initializingProperty == null;
            if (isInitializing)
                initializingProperty = ValueProperty;

            if (!interlock)
            {
                interlock = true;
                UpdateComponentsFromValue(Value);
                interlock = false;
            }

            UpdateBinding(ValueProperty);
            if (isInitializing)
                initializingProperty = null;
        }

        /// <summary>
        /// Raised when either of the <see cref="Days"/>, <see cref="Hours"/>, <see cref="Minutes"/> or <see cref="Seconds"/> properties are modified.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void OnComponentPropertyChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var isInitializing = !templateApplied && initializingProperty == null;
            if (isInitializing)
                initializingProperty = e.Property;

            if (!interlock)
            {
                interlock = true;
                Value = UpdateValueFromComponent(e.Property);
                UpdateComponentsFromValue(Value);
                interlock = false;
            }

            UpdateBinding(e.Property);
            if (isInitializing)
                initializingProperty = null;
        }

        /// <summary>
        /// Updates the binding of the given dependency property.
        /// </summary>
        /// <param name="dependencyProperty">The dependency property.</param>
        private void UpdateBinding(AvaloniaProperty dependencyProperty)
        {
            if (dependencyProperty != initializingProperty)
            {
                var expression = BindingOperations.GetBindingExpressionBase(this, dependencyProperty);
                expression?.UpdateSource();
            }
        }

        /// <summary>
        /// Raised by <see cref="DaysProperty"/>, <see cref="HoursProperty"/>, <see cref="MinutesProperty"/> or <see cref="SecondsProperty"/> when the <see cref="Days"/>, <see cref="Hours"/>, <see cref="Minutes"/> or <see cref="Seconds"/> dependency property is modified.
        /// </summary>
        /// <param name="sender">The dependency object where the event handler is attached.</param>
        /// <param name="e">The event data.</param>
        private static void OnComponentPropertyChanged(AvaloniaObject sender, AvaloniaPropertyChangedEventArgs e)
        {
            var editor = (TimeSpanEditor)sender;
            editor.OnComponentPropertyChanged(e);
        }

        /// <summary>
        /// Raised by <see cref="ValueProperty"/> when the <see cref="Value"/> dependency property is modified.
        /// </summary>
        /// <param name="sender">The dependency object where the event handler is attached.</param>
        /// <param name="e">The event data.</param>
        private static void OnValuePropertyChanged(AvaloniaObject sender, AvaloniaPropertyChangedEventArgs e)
        {
            var editor = (TimeSpanEditor)sender;
            editor.OnValueValueChanged();
        }
    }
}
