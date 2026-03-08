// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Globalization;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Controls;

namespace Stride.Core.Presentation.ValueConverters
{
    public class VectorEditingModeToBool : ValueConverterBase<VectorEditingModeToBool>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var toolMode = (VectorEditingMode)System.Convert.ChangeType(value, typeof(VectorEditingMode));
            return toolMode switch
            {
                VectorEditingMode.Normal => false,
                VectorEditingMode.AllComponents => true,
                VectorEditingMode.Length => null,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        [NotNull]
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return VectorEditingMode.Length;
            
            return ConverterHelper.ConvertToBoolean(value, culture) ? VectorEditingMode.AllComponents : VectorEditingMode.Normal;
        }
    }
}
