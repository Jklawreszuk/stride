// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Reactive;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Controls;
using Stride.Core.Presentation.Extensions;
using Stride.Core.Presentation.Interop;

namespace Stride.Core.Presentation.Behaviors
{
    public sealed class NumericTextBoxDragBehavior : MouseMoveCaptureBehaviorBase<NumericTextBox>
    {
        private enum DragState
        {
            None,
            Starting,
            Dragging,
        }

        private DragDirectionAdorner adorner;
        private Orientation dragOrientation;
        private DragState dragState;
        private Point mouseDownPosition;
        private double mouseMoveDelta;

        /// <summary>
        /// Identifies the <see cref="NumericTextBox.MouseValidationTrigger"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty DragCursorProperty = AvaloniaProperty.Register<NumericTextBox, Cursor>(nameof(DragCursor), new Cursor(StandardCursorType.SizeAll));
        
        /// <summary>
        /// Gets or sets the <see cref="Cursor"/> to display when the value can be modified via dragging.
        /// </summary>
        public Cursor DragCursor { get { return (Cursor)GetValue(DragCursorProperty); } set { SetValue(DragCursorProperty, value); } }

        /// <inheritdoc />
        protected override void CancelOverride()
        {
            AssociatedObject.Cursor = null;
            dragState = DragState.None;

            if (AssociatedObject.FindVisualRoot() is Control root)
            {
                root.Cursor = null;
                root.GetObservable(InputElement.IsKeyboardFocusWithinProperty)
                    .Subscribe(new AnonymousObserver<bool>(RootParentIsKeyboardFocusWithinChanged));
            }
        }

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Initialized += NumericTextBoxInitialized;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Initialized -= NumericTextBoxInitialized;
            if (AssociatedObject.contentHost != null)
                AssociatedObject.contentHost.PointerMoved -= HostQueryCursor;
        }

        /// <inheritdoc />
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (!IsContentHostPart(e.Source))
                return;

            if (!AssociatedObject.AllowMouseDrag || AssociatedObject.IsReadOnly || AssociatedObject.IsFocused)
                return;

            e.Handled = true;
            CaptureMouse(e);

            dragState = DragState.Starting;
            AssociatedObject.Cursor = new Cursor(StandardCursorType.None);
            mouseDownPosition = e.GetPosition(AssociatedObject);

            if (adorner == null)
            {
                adorner = new DragDirectionAdorner(AssociatedObject, AssociatedObject.contentHost.Bounds.Width);
                AdornerLayer.GetAdornerLayer(AssociatedObject);
                AdornerLayer.SetAdorner(AssociatedObject, adorner);
            }
        }

        /// <inheritdoc />
        protected override void OnMouseMove(PointerEventArgs e)
        {
            var position = e.GetPosition(AssociatedObject);
            if (AssociatedObject.AllowMouseDrag && dragState == DragState.Starting && e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
            {
                var dx = Math.Abs(position.X - mouseDownPosition.X);
                var dy = Math.Abs(position.Y - mouseDownPosition.Y);
                dragOrientation = dx >= dy ? Orientation.Horizontal : Orientation.Vertical;
                
                const int dragDistance = 4;
                if (dx > dragDistance || dy > dragDistance)
                {
                    if (AssociatedObject.FindVisualRoot() is Control root)
                    {
                        root.Cursor = null;
                        root.GetObservable(InputElement.IsKeyboardFocusWithinProperty)
                            .Subscribe(new AnonymousObserver<bool>(RootParentIsKeyboardFocusWithinChanged));
                    }

                    mouseDownPosition = position;
                    mouseMoveDelta = 0;
                    dragState = DragState.Dragging;
                    
                    AssociatedObject.SelectAll();
                    adorner?.SetOrientation(dragOrientation);
                }
            }

            if (dragState == DragState.Dragging)
            {
                if (dragOrientation == Orientation.Horizontal)
                    mouseMoveDelta += position.X - mouseDownPosition.X;
                else
                    mouseMoveDelta += mouseDownPosition.Y - position.Y;

                var deltaUsed = Math.Floor(mouseMoveDelta / NumericTextBox.DragSpeed);
                mouseMoveDelta -= deltaUsed;
                var newValue = (AssociatedObject.Value ?? 0.0) + deltaUsed * AssociatedObject.SmallChange;

                AssociatedObject.SetCurrentValue(NumericTextBox.ValueProperty, newValue);

                if (AssociatedObject.MouseValidationTrigger == MouseValidationTrigger.OnMouseMove)
                {
                    AssociatedObject.Validate();
                }

                mouseDownPosition = position;
            }
        }

        /// <inheritdoc />
        protected override void OnPointerReleased(PointerEventArgs e)
        {
            // We have to release the mouse first, in case Validate triggers a Detach of this behavior.
            ReleaseMouseCapture();
            if (dragState == DragState.Starting)
            {
                AssociatedObject.SelectAll();
                if (!AssociatedObject.IsFocused)
                {
                    AssociatedObject.Focus(NavigationMethod.Pointer);
                }
            }
            else if (dragState == DragState.Dragging && AssociatedObject.AllowMouseDrag)
            {
                if (adorner != null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
                    if (adornerLayer != null)
                    {
                        AdornerLayer.SetAdorner(AssociatedObject, null);
                        adorner = null;
                    }
                }
                AssociatedObject.Validate();
            }

            e.Handled = true;
            AssociatedObject.Cursor = null;
            dragState = DragState.None;
        }
        
        private void HostQueryCursor(object sender, PointerEventArgs e)
        {
            if (!IsContentHostPart(e.Source))
                return;

            if (!AssociatedObject.AllowMouseDrag || AssociatedObject.IsFocused || DragCursor == null)
                return;

            if (sender is Control control)
            {
                control.Cursor = DragCursor;
            }
            e.Handled = true;
        }
        
        private bool IsContentHostPart(object obj)
        {
            var frameworkElement = obj as Control;
            return Equals(obj, AssociatedObject.contentHost) || (frameworkElement != null && Equals(frameworkElement.Parent, AssociatedObject.contentHost));
        }
        
        private void NumericTextBoxInitialized(object sender, EventArgs e)
        {
            AssociatedObject.ApplyTemplate();
            AssociatedObject.contentHost.PointerMoved += HostQueryCursor;
        }

        private void RootParentIsKeyboardFocusWithinChanged(bool args)
        {
            // Cancel dragging in progress
            if (dragState == DragState.Dragging)
            {
                if (adorner != null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
                    if (adornerLayer != null)
                    {
                        AdornerLayer.SetAdorner(AssociatedObject, null);
                        adorner = null;
                    }
                }
                AssociatedObject.Cancel();
            }
            Cancel();
        }

        private class DragDirectionAdorner : Control
        {
            private readonly double contentWidth;
            private static readonly IImage CursorHorizontalImageSource;
            private static readonly IImage CursorVerticalImageSource;

            static DragDirectionAdorner()
            {
                var asmName = Assembly.GetExecutingAssembly().GetName().Name;
                CursorHorizontalImageSource = ImageExtensions.ImageSourceFromFile($"pack://application:,,,/{asmName};component/Resources/Images/cursor_west_east.png");
                CursorVerticalImageSource = ImageExtensions.ImageSourceFromFile($"pack://application:,,,/{asmName};component/Resources/Images/cursor_north_south.png");
            }

            private Orientation dragOrientation;
            private bool ready;

            internal DragDirectionAdorner([NotNull] Control adornedElement, double contentWidth)
            {
                this.contentWidth = contentWidth;
                IsHitTestVisible = false;
                Width = adornedElement.Bounds.Width;
                Height = adornedElement.Bounds.Height;
            }

            internal void SetOrientation(Orientation orientation)
            {
                ready = true;
                dragOrientation = orientation;
                InvalidateVisual();
            }

            public override void Render(DrawingContext drawingContext)
            {
                base.Render(drawingContext);

                if (!ready)
                    return;

                var source = dragOrientation == Orientation.Horizontal ? CursorHorizontalImageSource : CursorVerticalImageSource;
                var left = Math.Round(contentWidth - source.Size.Width);
                var top = Math.Round((Bounds.Height - source.Size.Height) * 0.5);
                drawingContext.DrawImage(source, new Rect(new Point(left, top), source.Size));
            }
        }
    }
}
