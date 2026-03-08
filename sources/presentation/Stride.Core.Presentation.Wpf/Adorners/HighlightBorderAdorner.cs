// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Stride.Core.Annotations;

namespace Stride.Core.Presentation.Adorners
{
    /// <summary>
    /// An adorner that draw a rectangle with borders over the adorned element. It can multiple possible states: Hidden, Visible, HighlightAccept and HighlightRefuse.
    /// </summary>
    public class HighlightBorderAdorner : Adorner
    {
        /// <summary>
        /// Identifies the <see cref="AcceptBorderBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty AcceptBorderBrushProperty = AvaloniaProperty.Register("AcceptBorderBrush", typeof(Brush), typeof(HighlightBorderAdorner), new PropertyMetadata(Brushes.PaleGreen, PropertyChanged));

        /// <summary>
        /// Identifies the <see cref="AcceptBorderBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty AcceptBorderThicknessProperty = AvaloniaProperty.Register("AcceptBorderThickness", typeof(double), typeof(HighlightBorderAdorner), new PropertyMetadata(2.0, PropertyChanged));

        /// <summary>
        /// Identifies the <see cref="AcceptBorderBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty AcceptBorderCornerRadiusProperty = AvaloniaProperty.Register("AcceptBorderCornerRadius", typeof(double), typeof(HighlightBorderAdorner), new PropertyMetadata(3.0, PropertyChanged));

        /// <summary>
        /// Identifies the <see cref="AcceptBorderBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty AcceptBackgroundBrushProperty = AvaloniaProperty.Register("AcceptBackgroundBrush", typeof(Brush), typeof(HighlightBorderAdorner), new PropertyMetadata(Brushes.MediumSeaGreen, PropertyChanged));

        /// <summary>
        /// Identifies the <see cref="AcceptBorderBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty AcceptBackgroundOpacityProperty = AvaloniaProperty.Register("AcceptBackgroundOpacity", typeof(double), typeof(HighlightBorderAdorner), new PropertyMetadata(0.3, PropertyChanged));

        /// <summary>
        /// Identifies the <see cref="RefuseBorderBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty RefuseBorderBrushProperty = AvaloniaProperty.Register("RefuseBorderBrush", typeof(Brush), typeof(HighlightBorderAdorner), new PropertyMetadata(Brushes.Red, PropertyChanged));

        /// <summary>
        /// Identifies the <see cref="RefuseBorderBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty RefuseBorderThicknessProperty = AvaloniaProperty.Register("RefuseBorderThickness", typeof(double), typeof(HighlightBorderAdorner), new PropertyMetadata(2.0, PropertyChanged));

        /// <summary>
        /// Identifies the <see cref="RefuseBorderBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty RefuseBorderCornerRadiusProperty = AvaloniaProperty.Register("RefuseBorderCornerRadius", typeof(double), typeof(HighlightBorderAdorner), new PropertyMetadata(3.0, PropertyChanged));

        /// <summary>
        /// Identifies the <see cref="RefuseBorderBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty RefuseBackgroundBrushProperty = AvaloniaProperty.Register("RefuseBackgroundBrush", typeof(Brush), typeof(HighlightBorderAdorner), new PropertyMetadata(Brushes.IndianRed, PropertyChanged));

        /// <summary>
        /// Identifies the <see cref="RefuseBorderBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty RefuseBackgroundOpacityProperty = AvaloniaProperty.Register("RefuseBackgroundOpacity", typeof(double), typeof(HighlightBorderAdorner), new PropertyMetadata(0.3, PropertyChanged));

        /// <summary>
        /// Identifies the <see cref="BorderBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty BorderBrushProperty = AvaloniaProperty.Register("BorderBrush", typeof(Brush), typeof(HighlightBorderAdorner), new PropertyMetadata(Brushes.SteelBlue, PropertyChanged));

        /// <summary>
        /// Identifies the <see cref="BorderThickness"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty BorderThicknessProperty = AvaloniaProperty.Register("BorderThickness", typeof(double), typeof(HighlightBorderAdorner), new PropertyMetadata(2.0, PropertyChanged));

        /// <summary>
        /// Identifies the <see cref="BorderCornerRadius"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty BorderCornerRadiusProperty = AvaloniaProperty.Register("BorderCornerRadius", typeof(double), typeof(HighlightBorderAdorner), new PropertyMetadata(3.0, PropertyChanged));

        /// <summary>
        /// Identifies the <see cref="BackgroundBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty BackgroundBrushProperty = AvaloniaProperty.Register("BackgroundBrush", typeof(Brush), typeof(HighlightBorderAdorner), new PropertyMetadata(Brushes.LightSteelBlue, PropertyChanged));

        /// <summary>
        /// Identifies the <see cref="BorderBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty BackgroundOpacityProperty = AvaloniaProperty.Register("BackgroundOpacity", typeof(double), typeof(HighlightBorderAdorner), new PropertyMetadata(0.3, PropertyChanged));

        /// <summary>
        /// Identifies the <see cref="State"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty StateProperty = AvaloniaProperty.Register("State", typeof(HighlightAdornerState), typeof(HighlightBorderAdorner), new PropertyMetadata(HighlightAdornerState.Hidden, PropertyChanged));

        /// <summary>
        /// Initializes a new instance of the <see cref="HighlightBorderAdorner"/> class.
        /// </summary>
        /// <param name="adornedElement"></param>
        public HighlightBorderAdorner([NotNull] Control adornedElement)
            : base(adornedElement)
        {
        }

        /// <summary>
        /// Gets or sets the border brush when the adorner is Accepted.
        /// </summary>
        public Brush AcceptBorderBrush { get { return (Brush)GetValue(AcceptBorderBrushProperty); } set { SetValue(AcceptBorderBrushProperty, value); } }

        /// <summary>
        /// Gets or sets the border thickness when the adorner is Accepted.
        /// </summary>
        public double AcceptBorderThickness { get { return (double)GetValue(AcceptBorderThicknessProperty); } set { SetValue(AcceptBorderThicknessProperty, value); } }

        /// <summary>
        /// Gets or sets the border corner radius when the adorner is Accepted.
        /// </summary>
        public double AcceptBorderCornerRadius { get { return (double)GetValue(AcceptBorderCornerRadiusProperty); } set { SetValue(AcceptBorderCornerRadiusProperty, value); } }

        /// <summary>
        /// Gets or sets the background brush when the adorner is Accepted.
        /// </summary>
        public Brush AcceptBackgroundBrush { get { return (Brush)GetValue(AcceptBackgroundBrushProperty); } set { SetValue(AcceptBackgroundBrushProperty, value); } }

        /// <summary>
        /// Gets or sets the background opacity when the adorner is Accepted.
        /// </summary>
        public double AcceptBackgroundOpacity { get { return (double)GetValue(AcceptBackgroundOpacityProperty); } set { SetValue(AcceptBackgroundOpacityProperty, value); } }

        /// <summary>
        /// Gets or sets the border brush when the adorner is Refuseed.
        /// </summary>
        public Brush RefuseBorderBrush { get { return (Brush)GetValue(RefuseBorderBrushProperty); } set { SetValue(RefuseBorderBrushProperty, value); } }

        /// <summary>
        /// Gets or sets the border thickness when the adorner is Refuseed.
        /// </summary>
        public double RefuseBorderThickness { get { return (double)GetValue(RefuseBorderThicknessProperty); } set { SetValue(RefuseBorderThicknessProperty, value); } }

        /// <summary>
        /// Gets or sets the border corner radius when the adorner is Refuseed.
        /// </summary>
        public double RefuseBorderCornerRadius { get { return (double)GetValue(RefuseBorderCornerRadiusProperty); } set { SetValue(RefuseBorderCornerRadiusProperty, value); } }

        /// <summary>
        /// Gets or sets the background brush when the adorner is Refuseed.
        /// </summary>
        public Brush RefuseBackgroundBrush { get { return (Brush)GetValue(RefuseBackgroundBrushProperty); } set { SetValue(RefuseBackgroundBrushProperty, value); } }

        /// <summary>
        /// Gets or sets the background opacity when the adorner is Refuseed.
        /// </summary>
        public double RefuseBackgroundOpacity { get { return (double)GetValue(RefuseBackgroundOpacityProperty); } set { SetValue(RefuseBackgroundOpacityProperty, value); } }

        /// <summary>
        /// Gets or sets the border brush when the adorner visible but not highlighted.
        /// </summary>
        public Brush BorderBrush { get { return (Brush)GetValue(BorderBrushProperty); } set { SetValue(BorderBrushProperty, value); } }

        /// <summary>
        /// Gets or sets the border thickness when the adorner is visible but not highlighted.
        /// </summary>
        public double BorderThickness { get { return (double)GetValue(BorderThicknessProperty); } set { SetValue(BorderThicknessProperty, value); } }

        /// <summary>
        /// Gets or sets the border corner radius when the adorner is visible but not highlighted.
        /// </summary>
        public double BorderCornerRadius { get { return (double)GetValue(BorderCornerRadiusProperty); } set { SetValue(BorderCornerRadiusProperty, value); } }

        /// <summary>
        /// Gets or sets the background brush when the adorner is visible but not highlighted.
        /// </summary>
        public Brush BackgroundBrush { get { return (Brush)GetValue(BackgroundBrushProperty); } set { SetValue(BackgroundBrushProperty, value); } }

        /// <summary>
        /// Gets or sets the background opacity when the adorner is visible but not highlighted.
        /// </summary>
        public double BackgroundOpacity { get { return (double)GetValue(BackgroundOpacityProperty); } set { SetValue(BackgroundOpacityProperty, value); } }
        
        /// <summary>
        /// Gets or sets the state of the adorner.
        /// </summary>
        public HighlightAdornerState State { get { return (HighlightAdornerState)GetValue(StateProperty); } set { SetValue(StateProperty, value); } }

        /// <inheritdoc/>
        protected override void OnRender(DrawingContext drawingContext)
        {
            var adornedElementRect = new Rect(AdornedElement.RenderSize);
            Brush renderBrush = null;
            Pen renderPen = null;
            switch (State)
            {
                case HighlightAdornerState.HighlightAccept:
                    if (AcceptBackgroundBrush != null)
                    {
                        renderBrush = AcceptBackgroundBrush.Clone();
                        renderBrush.Opacity = AcceptBackgroundOpacity;
                    }
                    if (AcceptBorderBrush != null)
                    {
                        renderPen = new Pen(AcceptBorderBrush, AcceptBorderThickness);
                    }
                    drawingContext.DrawRoundedRectangle(renderBrush, renderPen, adornedElementRect, AcceptBorderCornerRadius, AcceptBorderCornerRadius);
                    break;

                case HighlightAdornerState.HighlightRefuse:
                    if (RefuseBackgroundBrush != null)
                    {
                        renderBrush = RefuseBackgroundBrush.Clone();
                        renderBrush.Opacity = RefuseBackgroundOpacity;
                    }
                    if (RefuseBorderBrush != null)
                    {
                        renderPen = new Pen(RefuseBorderBrush, RefuseBorderThickness);
                    }
                    drawingContext.DrawRoundedRectangle(renderBrush, renderPen, adornedElementRect, RefuseBorderCornerRadius, RefuseBorderCornerRadius);
                    break;

                case HighlightAdornerState.Visible:
                    if (BackgroundBrush != null)
                    {
                        renderBrush = BackgroundBrush.Clone();
                        renderBrush.Opacity = BackgroundOpacity;
                    }
                    if (BorderBrush != null)
                    {
                        renderPen = new Pen(BorderBrush, BorderThickness);
                    }
                    drawingContext.DrawRoundedRectangle(renderBrush, renderPen, adornedElementRect, BorderCornerRadius, BorderCornerRadius);
                    break;
            }
        }

        private static void PropertyChanged([NotNull] AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var adorner = (Adorner)d;
            adorner.InvalidateVisual();
        }
    }
}
