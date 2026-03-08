// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

#region Copyright and license
// Some parts of this file were inspired by OxyPlot (https://github.com/oxyplot/oxyplot)
/*
The MIT license (MTI)
https://opensource.org/licenses/MIT

Copyright (c) 2014 OxyPlot contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal 
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is 
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Extensions;
using Stride.Core.Presentation.Internal;
using Stride.Core.Presentation.ValueConverters;

namespace Stride.Core.Presentation.Controls
{
    using MathUtil = Stride.Core.Mathematics.MathUtil;

    [TemplatePart(Name = HorizontalLinePartName, Type = typeof(Line))]
    [TemplatePart(Name = VerticalLinePartName, Type = typeof(Line))]
    public class TrackerControl : TemplatedControl
    {
        /// <summary>
        /// The name of the part for the horizontal line.
        /// </summary>
        private const string HorizontalLinePartName = "PART_HorizontalLine";

        /// <summary>
        /// The name of the part for  the vertical line.
        /// </summary>
        private const string VerticalLinePartName = "PART_VerticalLine";

        /// <summary>
        /// Identifies the <see cref="HorizontalLineVisibility"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty HorizontalLineVisibilityProperty =
            AvaloniaProperty.Register<TrackerControl,  bool>(nameof(HorizontalLineVisibility));

        /// <summary>
        /// Identifies the <see cref="LineExtents"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty LineExtentsProperty =
            AvaloniaProperty.Register<TrackerControl,  Rect>(nameof(LineExtents));

        /// <summary>
        /// Identifies the <see cref="LineStroke"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty LineStrokeProperty =
            AvaloniaProperty.Register<TrackerControl, Brush>(nameof(LineStroke));

        /// <summary>
        /// Identifies the <see cref="LineThickness"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty LineThicknessProperty =
            AvaloniaProperty.Register<TrackerControl, Thickness>(nameof(LineThickness));

        /// <summary>
        /// Identifies the <see cref="Position"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty PositionProperty =
            AvaloniaProperty.Register<TrackerControl, Point>(nameof(Position));

        /// <summary>
        /// Identifies the <see cref="TrackMouse"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty TrackMouseProperty =
            AvaloniaProperty.Register<TrackerControl, bool>(nameof(TrackMouse));

        /// <summary>
        /// Identifies the <see cref="VerticalLineVisibility"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty VerticalLineVisibilityProperty =
            AvaloniaProperty.Register<TrackerControl, bool>(nameof(VerticalLineVisibility));

        private Line horizontalLine;
        private Line verticalLine;
        private Control parent;

        static TrackerControl()
        {
            PositionProperty.Changed.AddClassHandler<TrackerControl>(OnPositionChanged);
            TrackMouseProperty.Changed.AddClassHandler<AvaloniaObject>(OnTrackMouseChanged);
        }

        public bool HorizontalLineVisibility { get { return (bool)GetValue(HorizontalLineVisibilityProperty); }  set { SetValue(HorizontalLineVisibilityProperty, value); } }
        
        public Rect LineExtents { get { return (Rect)GetValue(LineExtentsProperty); }  set { SetValue(LineExtentsProperty, value); } }

        public Brush LineStroke { get { return (Brush)GetValue(LineStrokeProperty); }  set { SetValue(LineStrokeProperty, value); } }

        public Thickness LineThickness { get { return (Thickness)GetValue(LineThicknessProperty); } set { SetValue(LineThicknessProperty, value); } }

        public Point Position { get { return (Point)GetValue(PositionProperty); } set { SetValue(PositionProperty, value); } }

        public bool TrackMouse { get { return (bool)GetValue(TrackMouseProperty); } set { SetValue(TrackMouseProperty, value.Box()); } }
        
        public bool VerticalLineVisibility { get { return (bool)GetValue(VerticalLineVisibilityProperty); }  set { SetValue(VerticalLineVisibilityProperty, value); } }

        private static void OnPositionChanged([NotNull] AvaloniaObject sender, AvaloniaPropertyChangedEventArgs e)
        {
            ((TrackerControl)sender).OnPositionChanged();
        }

        private static void OnTrackMouseChanged([NotNull] AvaloniaObject sender, AvaloniaPropertyChangedEventArgs e)
        {
            ((TrackerControl)sender).OnTrackMouseChanged(e);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            horizontalLine =  e.NameScope.Find<Line>(HorizontalLinePartName);
            verticalLine =  e.NameScope.Find<Line>(VerticalLinePartName);

            if (parent != null && TrackMouse)
                parent.PointerMoved -= OnMouseMove;
            parent = this.FindVisualParentOfType<Control>();
            if (TrackMouse)
                parent.PointerMoved += OnMouseMove;
        }

        private void OnMouseMove(object sender, [NotNull] PointerEventArgs e)
        {
            if (!TrackMouse)
                return;
            Position = e.GetPosition(this);
        }
        
        private void OnPositionChanged()
        {
            UpdatePositionAndBorder();
        }

        private void OnTrackMouseChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (parent == null)
                return;

            if ((bool)e.NewValue)
                parent.PointerMoved += OnMouseMove;
            else
                parent.PointerMoved -= OnMouseMove;
        }

        private void UpdatePositionAndBorder()
        {
            if (parent == null)
                return;

            var width = parent.Bounds.Width;
            var height = parent.Bounds.Height;
            var lineExtents = LineExtents;
            var pos = Position;

            if (horizontalLine != null)
            {
                double x1, x2;
                if (LineExtents.Width > 0)
                {
                    x1 = lineExtents.Left;
                    x2 = lineExtents.Right;
                    pos = pos.WithY(MathUtil.Clamp(pos.Y, lineExtents.Top, lineExtents.Bottom));
                }
                else
                {
                    x1 = 0;
                    x2 = width;
                }

                horizontalLine.StartPoint = new Point(x1, pos.Y);
                horizontalLine.EndPoint = new Point(x2, pos.Y);
            }

            if (verticalLine != null)
            {
                double y1, y2;
                if (LineExtents.Width > 0)
                {
                    y1 = lineExtents.Top;
                    y2 = lineExtents.Bottom;
                    pos = pos.WithX(MathUtil.Clamp(pos.X, lineExtents.Left, lineExtents.Right));
                }
                else
                {
                    y1 = 0;
                    y2 = height;
                }
                verticalLine.StartPoint = new Point(pos.X, y1);
                verticalLine.EndPoint = new Point(pos.X, y2);
            }
        }
    }
}
