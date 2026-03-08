// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Windows;
using Avalonia;
using Stride.Core.Mathematics;

namespace Stride.Core.Presentation.Controls
{
    public class Int2Editor : VectorEditorBase<Int2?>
    {
        /// <summary>
        /// Identifies the <see cref="X"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty XProperty = AvaloniaProperty.Register<Int2Editor, int?>("X");

        /// <summary>
        /// Identifies the <see cref="Y"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty YProperty = AvaloniaProperty.Register<Int2Editor, int?>("Y");

        /// <summary>
        /// Gets or sets the X component of the <see cref="Int2"/> associated to this control.
        /// </summary>
        public int? X { get { return (int?)GetValue(XProperty); } set { SetValue(XProperty, value); } }

        /// <summary>
        /// Gets or sets the Y component of the <see cref="Int2"/> associated to this control.
        /// </summary>
        public int? Y { get { return (int?)GetValue(YProperty); } set { SetValue(YProperty, value); } }

        static Int2Editor()
        {
            XProperty.Changed.AddClassHandler<Int2Editor>((sender, e) => OnComponentPropertyChanged(sender, e));
            YProperty.Changed.AddClassHandler<Int2Editor>((sender, e) => OnComponentPropertyChanged(sender, e));
        }

        /// <inheritdoc/>
        protected override void UpdateComponentsFromValue(Int2? value)
        {
            if (value != null)
            {
                SetCurrentValue(XProperty, value.Value.X);
                SetCurrentValue(YProperty, value.Value.Y);
            }
        }

        /// <inheritdoc/>
        protected override Int2? UpdateValueFromComponent(AvaloniaProperty property)
        {
            if (property == XProperty)
                return X.HasValue && Value.HasValue ? (Int2?)new Int2(X.Value, Value.Value.Y) : null;
            if (property == YProperty)
                return Y.HasValue && Value.HasValue ? (Int2?)new Int2(Value.Value.X, Y.Value) : null;
              
            throw new ArgumentException("Property unsupported by method UpdateValueFromComponent.");
        }

        /// <inheritdoc/>
        protected override Int2? UpdateValueFromFloat(float value)
        {
            return new Int2((int)Math.Round(value, MidpointRounding.AwayFromZero));
        }
    }
}
