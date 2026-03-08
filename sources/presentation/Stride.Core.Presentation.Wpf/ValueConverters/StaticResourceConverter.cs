// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace Stride.Core.Presentation.ValueConverters
{
    /// <summary>
    /// A converter that resolve the specified value from the resources from the current application
    /// </summary>
    public class StaticResourceConverter : OneWayValueConverter<StaticResourceConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return AvaloniaProperty.UnsetValue;

            return Application.Current?.TryFindResource(value, ThemeVariant.Default, out var resource) == true ? resource : AvaloniaProperty.UnsetValue;
        }
    }
}
