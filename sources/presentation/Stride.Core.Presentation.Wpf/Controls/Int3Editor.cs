// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Windows;
using Avalonia;
using Stride.Core.Mathematics;

namespace Stride.Core.Presentation.Controls
{
    public class Int3Editor : VectorEditorBase<Int3?>
    {
        /// <summary>
        /// Identifies the <see cref="X"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty XProperty = AvaloniaProperty.Register<Int3Editor, int?>("X");
            
        /// <summary>
        /// Identifies the <see cref="Y"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty YProperty = AvaloniaProperty.Register<Int3Editor, int?>("Y");
        /// <summary>
        /// Identifies the <see cref="Z"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ZProperty = AvaloniaProperty.Register<Int3Editor, int?>("Z");
        
        /// <summary>
        /// Gets or sets the X component of the <see cref="Int3"/> associated to this control.
        /// </summary>
        public int? X { get { return (int?)GetValue(XProperty); } set { SetValue(XProperty, value); } }

        /// <summary>
        /// Gets or sets the Y component of the <see cref="Int3"/> associated to this control.
        /// </summary>
        public int? Y { get { return (int?)GetValue(YProperty); } set { SetValue(YProperty, value); } }

        /// <summary>
        /// Gets or sets the Z component of the <see cref="Int3"/> associated to this control.
        /// </summary>
        public int? Z { get { return (int?)GetValue(ZProperty); } set { SetValue(ZProperty, value); } }

        static Int3Editor()
        {
            XProperty.Changed.AddClassHandler<Int3Editor>((sender, e) => OnComponentPropertyChanged(sender, e));
            YProperty.Changed.AddClassHandler<Int3Editor>((sender, e) => OnComponentPropertyChanged(sender, e));
            ZProperty.Changed.AddClassHandler<Int3Editor>((sender, e) => OnComponentPropertyChanged(sender, e));
        }
        
        /// <inheritdoc/>
        protected override void UpdateComponentsFromValue(Int3? value)
        {
            if (value != null)
            {
                SetCurrentValue(XProperty, value.Value.X);
                SetCurrentValue(YProperty, value.Value.Y);
                SetCurrentValue(ZProperty, value.Value.Z);
            }
        }

        /// <inheritdoc/>
        protected override Int3? UpdateValueFromComponent(AvaloniaProperty property)
        {
            if (property == XProperty)
                return X.HasValue && Value.HasValue ? (Int3?)new Int3(X.Value, Value.Value.Y, Value.Value.Z) : null;
            if (property == YProperty)
                return Y.HasValue && Value.HasValue ? (Int3?)new Int3(Value.Value.X, Y.Value, Value.Value.Z) : null;
            if (property == ZProperty)
                return Z.HasValue && Value.HasValue ? (Int3?)new Int3(Value.Value.X, Value.Value.Y, Z.Value) : null;

            throw new ArgumentException("Property unsupported by method UpdateValueFromComponent.");
        }

        /// <inheritdoc/>
        protected override Int3? UpateValueFromFloat(float value)
        {
            return new Int3((int)Math.Round(value, MidpointRounding.AwayFromZero));
        }
    }
}
