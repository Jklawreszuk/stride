// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Markup.Xaml.Templates;
using Stride.Core.Presentation.Internal;

namespace Stride.Core.Presentation.Controls
{
    public sealed class DateTimeEditor : TemplatedControl
    {
        private bool interlock;
        private bool templateApplied;
        private AvaloniaProperty initializingProperty;

        /// <summary>
        /// Identifies the <see cref="IsDropDownOpen"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty IsDropDownOpenProperty = AvaloniaProperty.Register<DateTimeEditor, bool>("IsDropDownOpen");

        /// <summary>
        /// Identifies the <see cref="WatermarkContent"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty WatermarkContentProperty = AvaloniaProperty.Register<DateTimeEditor, object>("WatermarkContent");

        /// <summary>
        /// Identifies the <see cref="WatermarkContentTemplate"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty WatermarkContentTemplateProperty = AvaloniaProperty.Register<DateTimeEditor, DataTemplate>("WatermarkContentTemplate");

        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ValueProperty = AvaloniaProperty.Register<DateTimeEditor, DateTime?>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

        /// <summary>
        /// Identifies the <see cref="Year"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty YearProperty = AvaloniaProperty.Register<DateTimeEditor, int?>(nameof(Year), defaultBindingMode: BindingMode.TwoWay);

        /// <summary>
        /// Identifies the <see cref="Month"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty MonthProperty = AvaloniaProperty.Register<DateTimeEditor, int?>(nameof(Month), defaultBindingMode: BindingMode.TwoWay);

        /// <summary>
        /// Identifies the <see cref="Day"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty DayProperty = AvaloniaProperty.Register<DateTimeEditor, int?>(nameof(Day), defaultBindingMode: BindingMode.TwoWay);

        /// <summary>
        /// Identifies the <see cref="Hour"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty HourProperty = AvaloniaProperty.Register<DateTimeEditor, int?>(nameof(Hour), defaultBindingMode: BindingMode.TwoWay);

        /// <summary>
        /// Identifies the <see cref="Minute"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty MinuteProperty = AvaloniaProperty.Register<DateTimeEditor, int?>(nameof(Minute), defaultBindingMode: BindingMode.TwoWay);

        /// <summary>
        /// Identifies the <see cref="Second"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty SecondProperty = AvaloniaProperty.Register<DateTimeEditor, double?>(nameof(Second), defaultBindingMode: BindingMode.TwoWay);

        /// <summary>
        /// Gets or sets whether the drop-down of this control editor is currently open.
        /// </summary>
        public bool IsDropDownOpen { get { return (bool)GetValue(IsDropDownOpenProperty); } set { SetValue(IsDropDownOpenProperty, value.Box()); } }

        /// <summary>
        /// Gets or sets the content to display when the TextBox is empty.
        /// </summary>
        public object WatermarkContent { get { return GetValue(WatermarkContentProperty); } set { SetValue(WatermarkContentProperty, value); } }

        /// <summary>
        /// Gets or sets the template of the content to display when the TextBox is empty.
        /// </summary>
        public DataTemplate WatermarkContentTemplate { get { return (DataTemplate)GetValue(WatermarkContentTemplateProperty); } set { SetValue(WatermarkContentTemplateProperty, value); } }

        /// <summary>
        /// Gets or sets the date time associated to this control.
        /// </summary>
        public DateTime? Value { get { return (DateTime?)GetValue(ValueProperty); } set { SetValue(ValueProperty, value); } }

        /// <summary>
        /// Gets or sets the year displayed in the <see cref="DateTimeEditor"/>.
        /// </summary>
        public int? Year { get { return (int?)GetValue(YearProperty); } set { SetValue(YearProperty, value); } }

        /// <summary>
        /// Gets or sets the month displayed in the <see cref="DateTimeEditor"/>.
        /// </summary>
        public int? Month { get { return (int?)GetValue(MonthProperty); } set { SetValue(MonthProperty, value); } }

        /// <summary>
        /// Gets or sets the day displayed in the <see cref="DateTimeEditor"/>.
        /// </summary>
        public int? Day { get { return (int?)GetValue(DayProperty); } set { SetValue(DayProperty, value); } }

        /// <summary>
        /// Gets or sets the hour displayed in the <see cref="DateTimeEditor"/>.
        /// </summary>
        public int? Hour { get { return (int?)GetValue(HourProperty); } set { SetValue(HourProperty, value); } }

        /// <summary>
        /// Gets or sets the minute displayed in the <see cref="DateTimeEditor"/>.
        /// </summary>
        public int? Minute { get { return (int?)GetValue(MinuteProperty); } set { SetValue(MinuteProperty, value); } }

        /// <summary>
        /// Gets or sets the second displayed in the <see cref="DateTimeEditor"/>.
        /// </summary>
        public double? Second { get { return (double?)GetValue(SecondProperty); } set { SetValue(SecondProperty, value); } }
        
        static DateTimeEditor()
        {
            ValueProperty.Changed.AddClassHandler<DateTimeEditor>((x, e) => x.OnValueChanged());
            YearProperty.Changed.AddClassHandler<DateTimeEditor>(OnComponentChanged);
            MonthProperty.Changed.AddClassHandler<DateTimeEditor>(OnComponentChanged);
            DayProperty.Changed.AddClassHandler<DateTimeEditor>(OnComponentChanged);
            HourProperty.Changed.AddClassHandler<DateTimeEditor>(OnComponentChanged);
            MinuteProperty.Changed.AddClassHandler<DateTimeEditor>(OnComponentChanged);
            SecondProperty.Changed.AddClassHandler<DateTimeEditor>(OnComponentChanged);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            templateApplied = false;
            base.OnApplyTemplate(e);
            templateApplied = true;
        }
        
        private static void OnComponentChanged(DateTimeEditor editor, AvaloniaPropertyChangedEventArgs e)
        {
            editor.OnComponentChanged(e);
        }
        
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IsKeyboardFocusWithinProperty)
            {
                if (IsDropDownOpen && !IsKeyboardFocusWithin)
                    SetCurrentValue(IsDropDownOpenProperty, false);
            }
        }
        
        private DateTime? UpdateValueFromComponent(AvaloniaProperty property)
        {
            if (!Value.HasValue)
                return null;

            var v = Value.Value;

            if (property == YearProperty && Year.HasValue)
                return new DateTime(Year.Value, v.Month, Math.Min(DateTime.DaysInMonth(Year.Value, v.Month), v.Day), v.Hour, v.Minute, v.Second);

            if (property == MonthProperty && Month.HasValue)
                return new DateTime(v.Year, Month.Value, Math.Min(DateTime.DaysInMonth(v.Year, Month.Value), v.Day), v.Hour, v.Minute, v.Second);

            if (property == DayProperty && Day.HasValue)
                return new DateTime(v.Year, v.Month, Math.Min(DateTime.DaysInMonth(v.Year, v.Month), Day.Value), v.Hour, v.Minute, v.Second);

            if (property == HourProperty && Hour.HasValue)
                return new DateTime(v.Year, v.Month, v.Day, Hour.Value, v.Minute, v.Second);

            if (property == MinuteProperty && Minute.HasValue)
                return new DateTime(v.Year, v.Month, v.Day, v.Hour, Minute.Value, v.Second);

            if (property == SecondProperty && Second.HasValue)
            {
                long ticks = v.Ticks - (v.Ticks % TimeSpan.TicksPerMinute);
                return new DateTime(ticks + (long)(Second.Value * TimeSpan.TicksPerSecond));
            }

            return null;
        }

        private void OnValueChanged()
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

            if (isInitializing)
                initializingProperty = null;
        }

        private void OnComponentChanged(AvaloniaPropertyChangedEventArgs e)
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

            if (isInitializing)
                initializingProperty = null;
        }

        private void UpdateComponentsFromValue(DateTime? value)
        {
            if (value == null)
                return;

            SetCurrentValue(YearProperty, value.Value.Year);
            SetCurrentValue(MonthProperty, value.Value.Month);
            SetCurrentValue(DayProperty, value.Value.Day);
            SetCurrentValue(HourProperty, value.Value.Hour);
            SetCurrentValue(MinuteProperty, value.Value.Minute);
            SetCurrentValue(SecondProperty,
                (double)(value.Value.Ticks % TimeSpan.TicksPerMinute) / TimeSpan.TicksPerSecond);
        }
    }
}
