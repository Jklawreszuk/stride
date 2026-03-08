// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Stride.Core.Annotations;
using Stride.Core.Diagnostics;
using Stride.Core.Presentation.Extensions;
using Stride.Core.Presentation.Interop;

namespace Stride.Core.Presentation.Windows
{
    /// <summary>
    /// A singleton class to manage the windows of an application and their relation to each other. It introduces the concept of blocking window,
    /// which can block the main window of the application but does not interact with modal windows.
    /// </summary>
    public class WindowManager : IDisposable
    {
        // TODO: this list should be completely external
        private static readonly string[] DebugWindowTypeNames =
        [
            // WPF adorners introduced in Visual Studio 2015 Update 2
            "Microsoft.XamlDiagnostics.WpfTap",
            // WPF Inspector
            "ChristianMoser.WpfInspector",
            // Snoop
            "Snoop.SnoopUI"
        ];

        private static readonly List<WindowInfo> ModalWindowsList = [];
        private static readonly List<WindowInfo> BlockingWindowsList = [];
        private static readonly HashSet<WindowInfo> AllWindowsList = [];

        // This must remains a field to prevent garbage collection!
        private static Dispatcher dispatcher;
        private static bool initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowManager"/> class.
        /// </summary>
        public WindowManager([NotNull] Dispatcher dispatcher)
        {
            if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));
            if (initialized) throw new InvalidOperationException("An instance of WindowManager is already existing.");

            initialized = true;
            WindowManager.dispatcher = dispatcher;
            
            Logger.Info($"{nameof(WindowManager)} initialized");
        }

#if DEBUG // Use a logger result for debugging
        public static Logger Logger { get; } = new LoggerResult();
#else
        public static Logger Logger { get; } = GlobalLogger.GetLogger(nameof(WindowManager));
#endif

        /// <summary>
        /// Gets the current main window.
        /// </summary>
        public static WindowInfo MainWindow { get; private set; }

        /// <summary>
        /// Gets the collection of currently visible modal windows.
        /// </summary>
        public static IReadOnlyList<WindowInfo> ModalWindows => ModalWindowsList;

        /// <summary>
        /// Gets the collection of currently visible blocking windows.
        /// </summary>
        public static IReadOnlyList<WindowInfo> BlockingWindows => BlockingWindowsList;

        /// <inheritdoc/>
        public void Dispose()
        {
            dispatcher = null;
            MainWindow = null;
            AllWindowsList.Clear();
            ModalWindowsList.Clear();
            BlockingWindowsList.Clear();

            Logger.Info($"{nameof(WindowManager)} disposed");
            initialized = false;
        }

        /// <summary>
        /// Shows the given window as the main window of the application. It is mandatory to use this method for the main window to use features of the <see cref="WindowManager"/>.
        /// </summary>
        /// <param name="window">The main window to show.</param>
        public static void ShowMainWindow([NotNull] Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));
            CheckDispatcher();

            if (MainWindow != null)
            {
                var message = "This application already has a main window.";
                Logger.Error(message);
                throw new InvalidOperationException(message);
            }
            Logger.Info($"Main window showing. ({window})");

            MainWindow = new WindowInfo(window);
            AllWindowsList.Add(MainWindow);

            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Show();
        }

        /// <summary>
        /// Shows the given window as blocking window. A blocking window will always block the main window of the application, even if shown before it, but does not
        /// affect modal windows. However, it can still be blocked by them..
        /// </summary>
        /// <param name="window">The blocking window to show.</param>
        public static void ShowBlockingWindow([NotNull] Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));
            CheckDispatcher();

            var windowInfo = new WindowInfo(window) { IsBlocking = true };
            if (BlockingWindowsList.Contains(windowInfo))
                throw new InvalidOperationException("This window has already been shown as blocking.");

            window.WindowStartupLocation = MainWindow != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen;

            // Set the owner now so the window can be recognized as modal when shown
            if (MainWindow != null)
            {
                MainWindow.IsDisabled = true;
            }

            AllWindowsList.Add(windowInfo);
            BlockingWindowsList.Add(windowInfo);

            // Update the hwnd on load in case the window is closed before being shown
            // We will receive EVENT_OBJECT_HIDE but not EVENT_OBJECT_SHOW in this case.
            window.Closed += (sender, e) => ActivateMainWindow();

            Logger.Info($"Modal window showing. ({window})");
            window.ShowDialog(MainWindow?.Window);
        }

        /// <summary>
        /// Displays the given window at the mouse cursor position when <see cref="Window.Show"/> will be called.
        /// </summary>
        /// <param name="window">The window to place at cursor position.</param>
        /// <remarks>This method must be called before <see cref="Window.Show"/>.</remarks>
        public static void ShowAtCursorPosition([NotNull] Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Loaded += PositionWindowToMouseCursor;
        }

        private static void PositionWindowToMouseCursor(object sender, RoutedEventArgs e)
        {
            var window = (Window)sender;
            // dispatch with one frame delay to make sure WPF layout passes are completed (if not, actual width and height might be incorrect)
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var area = window.GetWorkArea();
                if (area != new Rect())
                {
                    var mousePosition = window.GetCursorScreenPosition();
                    var expandRight = area.Right > mousePosition.X + window.Bounds.Width;
                    var expandBottom = area.Bottom > mousePosition.Y + window.Bounds.Height;
                    var x = expandRight ? mousePosition.X : mousePosition.X - window.Bounds.Width;
                    var y = expandBottom ? mousePosition.Y : mousePosition.Y - window.Bounds.Height;
                    window.Position = new PixelPoint((int)x, (int)y);
                }
            });

            window.Loaded -= PositionWindowToMouseCursor;
        }

        private static void ActivateMainWindow()
        {
            MainWindow?.Window.Activate();   
        }

        private static void CheckDispatcher()
        {
            if (!dispatcher.CheckAccess())
            {
                const string message = "This method must be invoked from the dispatcher thread";
                Logger.Error(message);
                throw new InvalidOperationException(message);
            }
        }

        [CanBeNull]
        internal static WindowInfo Find(WindowBase hwnd)
        {
            if (hwnd == null)
                return null;

            var result = AllWindowsList.FirstOrDefault(x => Equals(x.Owner, hwnd));
            return result != null ? result : AllWindowsList.FirstOrDefault(x => Equals(x.Window, hwnd));
        }
    }
}
