// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Avalonia;
using Avalonia.Controls;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Interop;

namespace Stride.Core.Presentation.Extensions
{
    /// <summary>
    /// Extension helpers for the <see cref="Window"/> class.
    /// </summary>
    public static class WindowHelper
    {
        /// <summary>
        /// Moves the <paramref cref="window"/> to the center of the given <paramref cref="area"/>.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="area">The area in WPF coordinates (see remarks).</param>
        /// <remarks>
        /// Because of monitor DPI, WPF screen coordinates and virtual screen coordinates can be different.
        /// To convert a <see cref="Rect"/> in virtual screen coordinates to WPF screen coordinates:
        /// <list type="number">
        /// <item>Use <see cref="VisualExtensions.RectFromScreen(System.Windows.Media.Visual,Rect)"/></item>
        /// <item>Offset the result <see cref="Rect"/> by the window top-left corner: <c>rect.Offset(window.Left, window.Top)</c></item>
        /// </list>
        /// To convert a <see cref="Rect"/> in WPF screen coordinates to virtual screen coordinates:
        /// <list type="number">
        /// <item>Un-offset the result <see cref="Rect"/> by the window top-left corner: <c>rect.Offset(-window.Left, -window.Top)</c></item>
        /// <item>Use <see cref="VisualExtensions.RectToScreen(System.Windows.Media.Visual,Rect)"/></item>
        /// </list>
        /// </remarks>
        public static void CenterToArea([NotNull] this Window window, Rect area)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));
            if (area.Width <= 0 || area.Height <= 0) return;

            var x = area.Left + Math.Abs(area.Width - window.Width) / 2;
            var y = area.Top + Math.Abs(area.Height - window.Height) / 2;

            window.Position = new PixelPoint((int)x, (int)y);
        }

        /// <summary>
        /// Gets the position of the cursor in the WPF screen coordinates of the given <paramref name="window"/>.
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method does not rely on <see cref="System.Windows.Input.Mouse"/> but calls native code to retrieve the position of the cursor.
        /// <br/>
        /// Because of monitor DPI, WPF screen coordinates and virtual screen coordinates can be different.
        /// To convert a <see cref="Point"/> in virtual screen coordinates to WPF screen coordinates:
        /// <list type="number">
        /// <item>Use <see cref="System.Windows.Media.Visual.PointFromScreen"/></item>
        /// <item>Offset the result <see cref="Point"/> by the window top-left corner: <c>point.Offset(window.Left, window.Top)</c></item>
        /// </list>
        /// To convert a <see cref="Point"/> in WPF screen coordinates to virtual screen coordinates:
        /// <list type="number">
        /// <item>Un-offset the result <see cref="Rect"/> by the window top-left corner: <c>point.Offset(-window.Left, -window.Top)</c></item>
        /// <item>Use <see cref="System.Windows.Media.Visual.PointToScreen"/></item>
        /// </list>
        /// </remarks>
        /// <seealso cref="NativeHelper.GetCursorPos"/>
        /// <seealso cref="System.Windows.Media.Visual.PointFromScreen"/>
        public static Point GetCursorScreenPosition([NotNull] this Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            return window.PointToClient(/* there is no way to get cursor position :( */
                new());
        }

        /// <summary>
        /// Gets the available work area for this <paramref cref="window"/> on the current screen.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <returns>The available work area on the current screen in WPF screen coordinates (see remarks).</returns>
        /// <remarks>
        /// Because of monitor DPI, WPF screen coordinates and virtual screen coordinates can be different.
        /// To convert a <see cref="Rect"/> in virtual screen coordinates to WPF screen coordinates:
        /// <list type="number">
        /// <item>Use <see cref="VisualExtensions.RectFromScreen(System.Windows.Media.Visual,Rect)"/></item>
        /// <item>Offset the result <see cref="Rect"/> by the window top-left corner: <c>rect.Offset(window.Left, window.Top)</c></item>
        /// </list>
        /// To convert a <see cref="Rect"/> in WPF screen coordinates to virtual screen coordinates:
        /// <list type="number">
        /// <item>Un-offset the result <see cref="Rect"/> by the window top-left corner: <c>rect.Offset(-window.Left, -window.Top)</c></item>
        /// <item>Use <see cref="VisualExtensions.RectToScreen(System.Windows.Media.Visual,Rect)"/></item>
        /// </list>
        /// </remarks>
        public static Rect GetWorkArea([NotNull] this Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            var screens = window.Screens;

            var screen = screens.ScreenFromWindow(window) ?? screens.Primary;

            if (screen == null)
                return new Rect();

            var workingArea = screen.WorkingArea;

            var scaling = screen.Scaling;
            var rect = new Rect(
                workingArea.X / scaling,
                workingArea.Y / scaling,
                workingArea.Width / scaling,
                workingArea.Height / scaling
            );

            return rect;
        }

        /// <summary>
        /// Moves and resize the <paramref cref="window"/> to make it fill the whole given <paramref cref="area"/>.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="area">The area in WPF coordinates (see remarks).</param>
        /// <remarks>
        /// Because of monitor DPI, WPF screen coordinates and virtual screen coordinates can be different.
        /// To convert a <see cref="Rect"/> in virtual screen coordinates to WPF screen coordinates:
        /// <list type="number">
        /// <item>Use <see cref="VisualExtensions.RectFromScreen(System.Windows.Media.Visual,Rect)"/></item>
        /// <item>Offset the result <see cref="Rect"/> by the window top-left corner: <c>rect.Offset(window.Left, window.Top)</c></item>
        /// </list>
        /// To convert a <see cref="Rect"/> in WPF screen coordinates to virtual screen coordinates:
        /// <list type="number">
        /// <item>Un-offset the result <see cref="Rect"/> by the window top-left corner: <c>rect.Offset(-window.Left, -window.Top)</c></item>
        /// <item>Use <see cref="VisualExtensions.RectToScreen(System.Windows.Media.Visual,Rect)"/></item>
        /// </list>
        /// </remarks>
        public static void FillArea([NotNull] this Window window, Rect area)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            window.Width = area.Width;
            window.Height = area.Height;
            window.Position = new PixelPoint((int)area.X, (int)area.Y);
        }

        #region Internals
        // FIXME: this should be turned private. Review usage in BehaviorProperties.
        [CanBeNull]
        internal static NativeHelper.MONITORINFO GetMonitorInfo(IntPtr hWnd)
        {
            var monitor = NativeHelper.MonitorFromWindow(hWnd, NativeHelper.MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new NativeHelper.MONITORINFO();
                NativeHelper.GetMonitorInfo(monitor, monitorInfo);
                return monitorInfo;
            }

            return null;
        }
        #endregion // Internals
    }
}
