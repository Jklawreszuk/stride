// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Interop;

namespace Stride.Core.Presentation.Commands
{
    /// <summary>
    /// A static class containing system commands to control a window.
    /// </summary>
    public static class SystemCommands
    {
        static SystemCommands()
        {
            MinimizeWindowCommand = new SystemCommand(CanMinimize, Minimize);
            MaximizeWindowCommand = new SystemCommand(CanMaximize, Maximize);
            RestoreWindowCommand = new SystemCommand(CanRestore, Restore);
            CloseWindowCommand = new SystemCommand(CanClose, Close);
            ShowSystemMenuCommand = new SystemCommand(CanShowSystemMenu, ShowSystemMenu);
        }

        /// <summary>
        /// Gets a command that minimizes the window passed as parameter.
        /// </summary>
        [NotNull]
        public static ICommand MinimizeWindowCommand { get;  }

        /// <summary>
        /// Gets a command that maximizes the window passed as parameter.
        /// </summary>
        [NotNull]
        public static ICommand MaximizeWindowCommand { get; }

        /// <summary>
        /// Gets a command that restores the window passed as parameter.
        /// </summary>
        [NotNull]
        public static ICommand RestoreWindowCommand { get; }

        /// <summary>
        /// Gets a command that closes the window passed as parameter.
        /// </summary>
        [NotNull]
        public static ICommand CloseWindowCommand { get; }

        /// <summary>
        /// Gets a command that show the system menu of the window passed as parameter.
        /// </summary>
        [NotNull]
        public static ICommand ShowSystemMenuCommand { get; }

        private static bool CanMinimize([NotNull] Window window)
        {
            return window.CanMinimize;
        }

        private static void Minimize([NotNull] Window window)
        {
            window.WindowState = WindowState.Minimized;
        }

        private static bool CanMaximize([NotNull] Window window)
        {
            return window.CanMaximize;
        }

        private static void Maximize([NotNull] Window window)
        {
            window.WindowState = WindowState.Maximized;
        }

        private static bool CanRestore([NotNull] Window window)
        {
            return window.CanMaximize;
        }

        private static void Restore([NotNull] Window window)
        {
            window.WindowState = WindowState.Normal;
        }

        private static bool CanClose([NotNull] Window window)
        {
            return true;
        }

        private static void Close([NotNull] Window window)
        {
            window.Close();
        }

        private static bool CanShowSystemMenu([NotNull] Window window)
        {
            return false;//not supported
        }

        private static void ShowSystemMenu([NotNull] Window window)
        {
            //TODO: DO WE REALLY NEED THIS ? 
            
            // Note: as we fetch for the content presenter, this command is a bit dependent of the window control template.
            // But both our template and the default (Aero) seems to use one so this is probably ok.
            // var presenter = window.FindVisualChildrenOfType<ContentPresenter>().FirstOrDefault(x => Equals(x.FindVisualParentOfType<Control>(), window));
            // if (presenter == null)
            //     throw new InvalidOperationException("The given window does not contain a ContentPresenter.");
            //
            // System.Windows.SystemCommands.ShowSystemMenu(window, presenter.PointToScreen(new Point(0, 0)));
        }
    }
}
