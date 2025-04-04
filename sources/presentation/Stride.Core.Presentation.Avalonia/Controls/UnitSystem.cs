// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;



namespace Stride.Core.Presentation.Controls
{
    /// <summary>
    /// Represent a unit system that can be used with a ScaleBar
    /// </summary>
    public class UnitSystem : AvaloniaObject
    {
        public static readonly StyledProperty<string> SymbolProperty = StyledProperty<string>.Register<UnitSystem, string>("Symbol"); // T1
        public static readonly StyledProperty<UnitGroupingCollection> GroupingValuesProperty = StyledProperty<UnitGroupingCollection>.Register<UnitSystem, UnitGroupingCollection>("GroupingValues"); // T1
        public static readonly StyledProperty<UnitConversionCollection> ConversionsProperty = StyledProperty<UnitConversionCollection>.Register<UnitSystem, UnitConversionCollection>("Conversions"); // T1

        /// <summary>
        /// The default symbol of the unit that will be appended to numeric value in the ScaleBar
        /// </summary>
        public string Symbol { get { return (string)GetValue(SymbolProperty); } set { SetValue(SymbolProperty, value); } }
        /// <summary>
        /// A list of <see cref="UnitGrouping" /> that can be used to override the default grouping method (which groups by the closest value of the form {1/2/5} * 10^n)
        /// </summary>
        public UnitGroupingCollection GroupingValues { get { return (UnitGroupingCollection)GetValue(GroupingValuesProperty); } set { SetValue(GroupingValuesProperty, value); } }
        /// <summary>
        /// A list of <see cref="UnitConversion" /> that can be used for grouping with a different unit (such as nano or mega units).
        /// </summary>
        public UnitConversionCollection Conversions { get { return (UnitConversionCollection)GetValue(ConversionsProperty); } set { SetValue(ConversionsProperty, value); } }

        public UnitSystem()
        {
            GroupingValues = new UnitGroupingCollection();
            Conversions = new UnitConversionCollection();
        }

        public void GetAllGroupingValues(ref List<double> values)
        {
            GroupingValues.GetAllGroupingValues(ref values, 1.0);
            Conversions.GetAllGroupingValues(ref values);
        }
    }

    /// <summary>
    /// Represent a unit conversion for an <see cref="UnitSystem" /> used for grouping large or small values (such as nano or mega units) 
    /// </summary>
    public class UnitConversion : AvaloniaObject
    {
        public static readonly StyledProperty<double> ValueProperty = StyledProperty<double>.Register<UnitConversion, double>("Value"); // T1
        public static readonly StyledProperty<string> SymbolProperty = StyledProperty<string>.Register<UnitConversion, string>("Symbol"); // T1
        public static readonly StyledProperty<UnitGroupingCollection> GroupingValuesProperty = StyledProperty<UnitGroupingCollection>.Register<UnitConversion, UnitGroupingCollection>("GroupingValues"); // T1
        public static readonly StyledProperty<bool> IsMultipliableProperty = StyledProperty<bool>.Register<UnitConversion, bool>("IsMultipliable"); // T1

        /// <summary>
        /// The value of this conversion expressed in its parent unit
        /// </summary>
        public double Value { get { return (double)GetValue(ValueProperty); } set { SetValue(ValueProperty, value); } }
        /// <summary>
        /// The symbol of this conversion
        /// </summary>
        public string Symbol { get { return (string)GetValue(SymbolProperty); } set { SetValue(SymbolProperty, value); } }
        /// <summary>
        /// A list of <see cref="UnitGrouping" /> that can be used to override the default grouping method (which groups by the closest value of the form {1/2/5} * 10^n)
        /// </summary>
        public UnitGroupingCollection GroupingValues { get { return (UnitGroupingCollection)GetValue(GroupingValuesProperty); } set { SetValue(GroupingValuesProperty, value); } }
        /// <summary>
        /// If the <see cref="GroupingValues" /> list is empty, indicate that the values can be grouped using the default grouping method  (which groups by the closest value of the form {1/2/5} * 10^n * <see cref="Value" />)
        /// </summary>
        public bool IsMultipliable { get { return (bool)GetValue(IsMultipliableProperty); } set { SetValue(IsMultipliableProperty, value); } }

        public UnitConversion()
        {
            GroupingValues = new UnitGroupingCollection();
        }
    }

    /// <summary>
    /// Represent an acceptable value for grouping units
    /// </summary>
    public class UnitGrouping : AvaloniaObject
    {
        public static readonly StyledProperty<double> LargeIntervalSizeProperty = StyledProperty<double>.Register<UnitGrouping, double>("LargeIntervalSize"); // T1
        public static readonly StyledProperty<int> SmallIntervalCountProperty = StyledProperty<int>.Register<UnitGrouping, int>("SmallIntervalCount", 10); // T6
        public static readonly StyledProperty<bool> IsMultipliableProperty = StyledProperty<bool>.Register<UnitGrouping, bool>("IsMultipliable"); // T1

        /// <summary>
        /// Grouping value that represent the length of a large tick interval in a ScaleBar
        /// </summary>
        public double LargeIntervalSize { get { return (double)GetValue(LargeIntervalSizeProperty); } set { SetValue(LargeIntervalSizeProperty, value); } }
        /// <summary>
        /// Number of small intervals (generating small ticks) into a large tick interval of a ScaleBar
        /// </summary>
        public int SmallIntervalCount { get { return (int)GetValue(SmallIntervalCountProperty); } set { SetValue(SmallIntervalCountProperty, value); } }
        /// <summary>
        /// Indicate that the values can be grouped more efficiently using the default grouping method (which groups by the closest value of the form {1/2/5} * 10^n * <see cref="LargeIntervalSize" />)
        /// </summary>
        public bool IsMultipliable { get { return (bool)GetValue(IsMultipliableProperty); } set { SetValue(IsMultipliableProperty, value); } }

        public UnitGrouping() { }

        public UnitGrouping(double largeIntervalSize, int smallIntervalCount)
        {
            LargeIntervalSize = largeIntervalSize;
            SmallIntervalCount = smallIntervalCount;
        }
    }

    /// <summary>
    /// A collection of <see cref="UnitConversion" />. Note that when two multipliable <see cref="UnitConversion" /> conflict (eg. 5mm and 0.5cm), the last one has priority.
    /// </summary>
    public class UnitConversionCollection : List<UnitConversion>
    {
        public void GetAllGroupingValues(ref List<double> values)
        {
            foreach (UnitConversion conversion in this)
            {
                conversion.GroupingValues.GetAllGroupingValues(ref values, conversion.Value);
            }
        }
    }

    /// <summary>
    /// A collection of <see cref="UnitGrouping" />.Note that when two multipliable <see cref="UnitGrouping" /> conflict, the last one has priority.
    /// </summary>
    public class UnitGroupingCollection : List<UnitGrouping>
    {
        public void GetAllGroupingValues(ref List<double> values, double multiplier)
        {
            foreach (var value in this.Select(grouping => grouping.LargeIntervalSize * multiplier))
            {
                if (!values.Contains(value))
                    values.Add(value);
            }
            if (Count == 0)
            {
                if (!values.Contains(multiplier))
                    values.Add(multiplier);
            }

        }
    }
}
