// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using Avalonia.Controls;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Reactive;
using Avalonia.Styling;

namespace Stride.Core.Presentation.Behaviors
{
    public sealed class SelectionRectangleBehavior : MouseMoveCaptureBehaviorBase<Control>
    {
        public static readonly AvaloniaProperty CanvasProperty =
            AvaloniaProperty.Register<SelectionRectangleBehavior, Canvas>(nameof(Canvas));

        public static readonly AvaloniaProperty CommandProperty =
            AvaloniaProperty.Register<SelectionRectangleBehavior, ICommand>(nameof(Command));

        public static readonly AvaloniaProperty RectangleStyleProperty =
            AvaloniaProperty.Register<SelectionRectangleBehavior, Style>(nameof(RectangleStyle));

        private Point originPoint;
        private Rectangle selectionRectangle;
        
        /// <summary>
        /// Resource Key for the default SelectionRectangleStyle.
        /// </summary>
        public static ResourceKey DefaultRectangleStyleKey { get; } = new ComponentResourceKey(typeof(SelectionRectangleBehavior), nameof(DefaultRectangleStyleKey));

        public Canvas Canvas { get { return (Canvas)GetValue(CanvasProperty); } set { SetValue(CanvasProperty, value); } }

        public ICommand Command { get { return (ICommand)GetValue(CommandProperty); } set { SetValue(CommandProperty, value); } }

        public Style RectangleStyle { get { return (Style)GetValue(RectangleStyleProperty); } set { SetValue(RectangleStyleProperty, value); } }
        
        public bool IsDragging { get; private set; }

        static SelectionRectangleBehavior()
        {
            CanvasProperty.Changed.AddClassHandler<AvaloniaObject>(OnCanvasChanged);
        }
        
        private static void OnCanvasChanged(AvaloniaObject obj, AvaloniaPropertyChangedEventArgs e)
        {
            var behavior = (SelectionRectangleBehavior)obj;
            behavior.OnCanvasChanged(e);
        }

        ///  <inheritdoc/>
        protected override void CancelOverride()
        {
            IsDragging = false;
            Canvas.IsVisible = false;
        }

        ///  <inheritdoc/>
        protected override void OnMouseDown(PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(AssociatedObject).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed)
                return;

            e.Handled = true;
            CaptureMouse(e);
            
            originPoint = e.GetPosition(AssociatedObject);
        }

        ///  <inheritdoc/>
        protected override void OnMouseMove(PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
            {
                Cancel();
                return;
            }

            var point = e.GetPosition(AssociatedObject);
            if (IsDragging)
            {
                UpdateDragSelectionRect(originPoint, point);
                e.Handled = true;
            }
            else
            {
                const double DragThreshold = 4;
                var curMouseDownPoint = e.GetPosition(AssociatedObject);
                var dragDelta = curMouseDownPoint - originPoint;
                if (Math.Abs(dragDelta.X) > DragThreshold ||
                    Math.Abs(dragDelta.Y) > DragThreshold)
                {
                    IsDragging = true;
                    InitDragSelectionRect(originPoint, curMouseDownPoint);
                }
                e.Handled = true;
            }
        }

        ///  <inheritdoc/>
        protected override void OnMouseUp(PointerEventArgs e)
        {
            if (e.GetCurrentPoint(AssociatedObject).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed)
                return;

            e.Handled = true;
            ReleaseMouseCapture();

            if (!IsDragging)
                return;

            IsDragging = false;
            ApplyDragSelectionRect();
        }

        private void CreateSelectionRectangle()
        {
            selectionRectangle = new Rectangle();
            if (RectangleStyle != null)
            {
                var binding = new Binding
                {
                    Path = new(nameof(RectangleStyle)),
                    Source = this,
                };
                selectionRectangle.Bind(Rectangle.StyleProperty, binding);
            }
            else
            {
                selectionRectangle.Style = selectionRectangle?.TryFindResource(DefaultRectangleStyleKey) as Style;
            }
            selectionRectangle.IsHitTestVisible = false;
        }

        public void OnCanvasChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.OldValue is Canvas oldCanvas && selectionRectangle != null)
            {
                oldCanvas.Children.Remove(selectionRectangle);
            }

            if (e.NewValue is not Canvas newCanvas)
                return;
            newCanvas.IsVisible = false;

            if (selectionRectangle == null)
                CreateSelectionRectangle();
            if (selectionRectangle != null)
                newCanvas.Children.Add(selectionRectangle);
        }

        /// <summary>
        /// Initialize the rectangle used for drag selection.
        /// </summary>
        private void InitDragSelectionRect(Point pt1, Point pt2)
        {
            UpdateDragSelectionRect(pt1, pt2);
            Canvas.IsVisible = true;
        }

        /// <summary>
        /// Update the position and size of the rectangle used for drag selection.
        /// </summary>
        private void UpdateDragSelectionRect(Point pt1, Point pt2)
        {
            double x, y, width, height;

            //
            // Determine x,y,width and height of the rect inverting the points if necessary.
            // 

            if (pt2.X < pt1.X)
            {
                x = pt2.X;
                width = pt1.X - pt2.X;
            }
            else
            {
                x = pt1.X;
                width = pt2.X - pt1.X;
            }

            if (pt2.Y < pt1.Y)
            {
                y = pt2.Y;
                height = pt1.Y - pt2.Y;
            }
            else
            {
                y = pt1.Y;
                height = pt2.Y - pt1.Y;
            }

            //
            // Update the coordinates of the rectangle used for drag selection.
            //
            Canvas.SetLeft(selectionRectangle, x);
            Canvas.SetTop(selectionRectangle, y);
            selectionRectangle.Width = width;
            selectionRectangle.Height = height;
        }

        /// <summary>
        /// Select all nodes that are in the drag selection rectangle.
        /// </summary>
        private void ApplyDragSelectionRect()
        {
            Canvas.IsVisible = false;

            if (Command == null)
                return;

            var x = Canvas.GetLeft(selectionRectangle);
            var y = Canvas.GetTop(selectionRectangle);
            var width = selectionRectangle.Width;
            var height = selectionRectangle.Height;
            var dragRect = new Rect(x, y, width, height);
            
            if (Command.CanExecute(dragRect))
            {
                Command.Execute(dragRect);
            }
        }
    }
}
