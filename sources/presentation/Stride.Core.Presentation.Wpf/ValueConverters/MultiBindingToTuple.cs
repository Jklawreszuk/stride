// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Globalization;
using Stride.Core.Annotations;

namespace Stride.Core.Presentation.ValueConverters
{
    public class MultiBindingToTuple : OneWayMultiValueConverter<MultiBindingToTuple>
    {
        [NotNull]
        public override object Convert([NotNull] IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Count switch
            {
                2 => new Tuple<object, object>(values[0], values[1]),
                3 => new Tuple<object, object, object>(values[0], values[1], values[2]),
                4 => new Tuple<object, object, object, object>(values[0], values[1], values[2], values[3]),
                5 => new Tuple<object, object, object, object, object>(values[0], values[1], values[2], values[3], values[4]),
                6 => new Tuple<object, object, object, object, object, object>(values[0], values[1], values[2], values[3], values[4], values[5]),
                7 => new Tuple<object, object, object, object, object, object, object>(values[0], values[1], values[2], values[3], values[4], values[5], values[6]),
                8 => new Tuple<object, object, object, object, object, object, object, object>(values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7]),
                _ => throw new ArgumentException("This converter supports only between two and eight elements")
            };
        }
    }
}
