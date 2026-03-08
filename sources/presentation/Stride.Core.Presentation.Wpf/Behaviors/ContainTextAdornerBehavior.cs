// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Xaml.Interactivity;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Adorners;
using Stride.Core.Presentation.Core;
using Stride.Core.Presentation.Extensions;

namespace Stride.Core.Presentation.Behaviors
{
    public class ContainTextAdornerBehavior : Behavior<TextBox>
    {
        private readonly DependencyPropertyWatcher propertyWatcher = new();
        private HighlightBorderAdorner adorner;

        /// <summary>
        /// Identifies the <see cref="BorderBrush"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty BorderBrushProperty = 
            AvaloniaProperty.Register<ContainTextAdornerBehavior,IBrush?>(nameof(BorderBrush), Brushes.SteelBlue);
        /// <summary>
        /// Identifies the <see cref="BorderCornerRadius"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty BorderCornerRadiusProperty =
            AvaloniaProperty.Register<ContainTextAdornerBehavior,double>(nameof(BorderCornerRadius), 3.0);
        /// <summary>
        /// Identifies the <see cref="BorderThickness"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty BorderThicknessProperty =
            AvaloniaProperty.Register<ContainTextAdornerBehavior,double>(nameof(BorderThickness), 2.0);
        
        /// <summary>
        /// Gets or sets the border brush when the adorner visible.
        /// </summary>
        public Brush BorderBrush { get { return (Brush)GetValue(BorderBrushProperty); } set { SetValue(BorderBrushProperty, value); } }
        /// <summary>
        /// Gets or sets the border corner radius when the adorner is visible.
        /// </summary>
        public double BorderCornerRadius { get { return (double)GetValue(BorderCornerRadiusProperty); } set { SetValue(BorderCornerRadiusProperty, value); } }
        /// <summary>
        /// Gets or sets the border thickness when the adorner is visible.
        /// </summary>
        public double BorderThickness { get { return (double)GetValue(BorderThicknessProperty); } set { SetValue(BorderThicknessProperty, value); } }

        protected override void OnAttached()
        {
            var textProperty = AssociatedObject.GetDependencyProperties(true).FirstOrDefault(dp => dp.Name == nameof(AssociatedObject.Text));
            if (textProperty == null)
                throw new ArgumentException($"Unable to find public property '{nameof(AssociatedObject.Text)}' on object of type '{AssociatedObject.GetType().FullName}'.");

            propertyWatcher.Attach(AssociatedObject);
            propertyWatcher.RegisterValueChangedHandler(textProperty, OnTextChanged);
            
            var adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
            if (adornerLayer != null)
            {
                adorner = new HighlightBorderAdorner()
                {
                    Child = AssociatedObject,
                    BackgroundBrush = null,
                    BorderBrush = BorderBrush,
                    BorderCornerRadius = BorderCornerRadius,
                    BorderThickness = BorderThickness,
                    State = HighlightAdornerState.Hidden,
                };
                AdornerLayer.SetAdorner(AssociatedObject, adorner);
            }
        }

        protected override void OnDetaching()
        {
            propertyWatcher.Detach();
            
            if (adorner != null)
            {
                AdornerLayer.SetAdorner(AssociatedObject, null);
                adorner = null;;
            }
        }

        private static void PropertyChanged([NotNull] AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var behavior = (ContainTextAdornerBehavior)d;
            var adorner = behavior.adorner;
            if (adorner != null)
            {
                if (e.Property == BorderBrushProperty)
                    adorner.BorderBrush = behavior.BorderBrush;

                if (e.Property == BorderCornerRadiusProperty)
                    adorner.BorderCornerRadius = behavior.BorderCornerRadius;

                if (e.Property == BorderThicknessProperty)
                    adorner.BorderThickness = behavior.BorderThickness;
            }
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            if (adorner == null)
                return;

            var showAdorner = !string.IsNullOrEmpty(AssociatedObject.Text);
            adorner.State = showAdorner ? HighlightAdornerState.Visible : HighlightAdornerState.Hidden;
        }
    }
}
