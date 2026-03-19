// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Services;
using Stride.Core.Presentation.Windows;

namespace Stride.Core.Presentation.Controls
{
    /// <summary>
    /// A window that show at the mouse cursor location, has no title bar, and is closed (with <see cref="Services.DialogResult.Cancel"/> result) when the
    /// user clicks outside of it or presses Escape.
    /// </summary>
    /// <remarks>
    /// This window will capture mouse. When handling mouse events, <see cref="IsMouseOverWindow"/> can be used to check whether the mouse event
    /// occurred inside the window.
    /// </remarks>
    public abstract class PopupModalWindow : ModalWindow
    {
        private bool closing;

        protected PopupModalWindow()
        {
            Loaded += OnLoaded;
            Deactivated += OnDeactivated;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!IsPointerCaptured)
                Mouse.Capture(this, CaptureMode.SubTree);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            if ( e.NameScope.Find<Control>("TitleBar") is { } titleBar)
                titleBar.IsVisible = false;
        }

        public override Task<DialogResult> ShowModal()
        {
            WindowManager.ShowAtCursorPosition(this);
            return base.ShowModal();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                CloseWithCancel();
                e.Handled = true;
            }
        }

        protected void OnDeactivated(object sender, EventArgs e)
        {
            CloseWithCancel();
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            if (!e.Cancel)
                closing = true;

            base.OnClosing(e);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (!IsMouseOverWindow(e))
            {
                CloseWithCancel();
                e.Handled = true;
            }
        }

        private void CloseWithCancel()
        {
            if (!closing)
            {
                Result = DialogResult.Cancel;
                Close();
            }
        }

        protected bool IsMouseOverWindow([NotNull] PointerEventArgs e)
        {
            var position = e.GetPosition(this);
            return position.X >= 0 && position.Y >= 0 && position.X < Bounds.Width && position.Y < Bounds.Height;
        }
    }
}
