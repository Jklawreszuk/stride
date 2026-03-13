// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System.Collections.Generic;
using Avalonia;
using Matrix = Stride.Core.Mathematics.Matrix;

namespace Stride.Core.Presentation.Controls
{
    public class MatrixEditor : VectorEditorBase<Matrix?>
    {
        private static readonly Dictionary<AvaloniaProperty, int> PropertyToIndex;

        /// <summary>
        /// Identifies the <see cref="M11"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty M11Property = AvaloniaProperty.Register<MatrixEditor, float?>("M11");

        /// <summary>
        /// Identifies the <see cref="M12"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty M12Property = AvaloniaProperty.Register<MatrixEditor, float?>("M12");
        
        /// <summary>
        /// Identifies the <see cref="M13"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty M13Property = AvaloniaProperty.Register<MatrixEditor, float?>("M13");
        
        /// <summary>
        /// Identifies the <see cref="M14"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty M14Property = AvaloniaProperty.Register<MatrixEditor, float?>("M14");
        
        /// <summary>
        /// Identifies the <see cref="M21"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty M21Property = AvaloniaProperty.Register<MatrixEditor, float?>("M21");
        
        /// <summary>
        /// Identifies the <see cref="M22"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty M22Property = AvaloniaProperty.Register<MatrixEditor, float?>("M22");
        
        /// <summary>
        /// Identifies the <see cref="M23"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty M23Property = AvaloniaProperty.Register<MatrixEditor, float?>("M23");
        
        /// <summary>
        /// Identifies the <see cref="M24"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty M24Property = AvaloniaProperty.Register<MatrixEditor, float?>("M24");
        
        /// <summary>
        /// Identifies the <see cref="M31"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty M31Property = AvaloniaProperty.Register<MatrixEditor, float?>("M31");
        
        /// <summary>
        /// Identifies the <see cref="M32"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty M32Property = AvaloniaProperty.Register<MatrixEditor, float?>("M32");
        
        /// <summary>
        /// Identifies the <see cref="M33"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty M33Property = AvaloniaProperty.Register<MatrixEditor, float?>("M33");
        
        /// <summary>
        /// Identifies the <see cref="M34"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty M34Property = AvaloniaProperty.Register<MatrixEditor, float?>("M34");
        
        /// <summary>
        /// Identifies the <see cref="M41"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty M41Property = AvaloniaProperty.Register<MatrixEditor, float?>("M41");
        
        /// <summary>
        /// Identifies the <see cref="M42"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty M42Property = AvaloniaProperty.Register<MatrixEditor, float?>("M42");
        
        /// <summary>
        /// Identifies the <see cref="M43"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty M43Property = AvaloniaProperty.Register<MatrixEditor, float?>("M43");
        
        /// <summary>
        /// Identifies the <see cref="M44"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty M44Property = AvaloniaProperty.Register<MatrixEditor, float?>("M44");
        
        static MatrixEditor()
        {
            PropertyToIndex = new Dictionary<AvaloniaProperty, int> {
                { M11Property, 0 }, { M12Property, 1 }, { M13Property, 2 }, { M14Property, 3 },
                { M21Property, 4 }, { M22Property, 5 }, { M23Property, 6 }, { M24Property, 7 },
                { M31Property, 8 }, { M32Property, 9 }, { M33Property, 10 }, { M34Property, 11 },
                { M41Property, 12 }, { M42Property, 13 }, { M43Property, 14 }, { M44Property, 15 },
            };
        }
    
        /// <summary>
        /// The value at the first column of the first row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float? M11 { get { return (float?)GetValue(M11Property); } set { SetValue(M11Property, value); } }

        /// <summary>
        /// The value at the second column of the first row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float? M12 { get { return (float?)GetValue(M12Property); } set { SetValue(M12Property, value); } }

        /// <summary>
        /// The value at the third column of the first row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float? M13 { get { return (float?)GetValue(M13Property); } set { SetValue(M13Property, value); } }

        /// <summary>
        /// The value at the fourth column of the first row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float? M14 { get { return (float?)GetValue(M14Property); } set { SetValue(M14Property, value); } }

        /// <summary>
        /// The value at the first column of the second row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float? M21 { get { return (float?)GetValue(M21Property); } set { SetValue(M21Property, value); } }

        /// <summary>
        /// The value at the second column of the second row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float? M22 { get { return (float?)GetValue(M22Property); } set { SetValue(M22Property, value); } }

        /// <summary>
        /// The value at the third column of the second row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float? M23 { get { return (float?)GetValue(M23Property); } set { SetValue(M23Property, value); } }

        /// <summary>
        /// The value at the fourth column of the second row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float? M24 { get { return (float?)GetValue(M24Property); } set { SetValue(M24Property, value); } }

        /// <summary>
        /// The value at the first column of the third row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float? M31 { get { return (float?)GetValue(M31Property); } set { SetValue(M31Property, value); } }

        /// <summary>
        /// The value at the second column of the third row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float? M32 { get { return (float?)GetValue(M32Property); } set { SetValue(M32Property, value); } }

        /// <summary>
        /// The value at the third column of the third row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float? M33 { get { return (float?)GetValue(M33Property); } set { SetValue(M33Property, value); } }

        /// <summary>
        /// The value at the fourth column of the third row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float? M34 { get { return (float?)GetValue(M34Property); } set { SetValue(M34Property, value); } }

        /// <summary>
        /// The value at the first column of the fourth row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float? M41 { get { return (float?)GetValue(M41Property); } set { SetValue(M41Property, value); } }

        /// <summary>
        /// The value at the second column of the fourth row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float? M42 { get { return (float?)GetValue(M42Property); } set { SetValue(M42Property, value); } }

        /// <summary>
        /// The value at the third column of the fourth row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float? M43 { get { return (float?)GetValue(M43Property); } set { SetValue(M43Property, value); } }

        /// <summary>
        /// The value at the fourth column of the fourth row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float? M44 { get { return (float?)GetValue(M44Property); } set { SetValue(M44Property, value); } }

        /// <inheritdoc/>
        protected override void UpdateComponentsFromValue(Matrix? value)
        {
            if (value != null)
            {
                foreach (var property in PropertyToIndex)
                {
                    SetCurrentValue(property.Key, value.Value[property.Value]);
                }
            }
        }

        /// <inheritdoc/>
        protected override Matrix? UpdateValueFromComponent(AvaloniaProperty property)
        {
            if (!Value.HasValue || !((float?)GetValue(property)).HasValue)
                return null;

            var array = new float[16];
            foreach (var dependencyProperty in PropertyToIndex)
            {
                array[dependencyProperty.Value] = property == dependencyProperty.Key ? ((float?)GetValue(dependencyProperty.Key)).Value : Value.Value[dependencyProperty.Value];
            }
            return new Matrix(array);
        }

        protected override Matrix? UpateValueFromFloat(float value)
        {
            return new Matrix(value);
        }
    }
}
