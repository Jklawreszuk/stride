// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace Stride.Core.Presentation.Interop
{
    /// <summary>
    /// Enables to register listener to the native clipboard changed event (also called clipboard viewers)
    /// </summary>
    public static class ClipboardMonitor
    {
        private static IntPtr hwndNextViewer;

        /// <summary>
        /// Raised when the clipboard has changed and contains text.
        /// </summary>
        /// <remarks>The sender of this event a window that was previously registered as a clipboard viewer with <see cref="RegisterListener"/>.</remarks>
        public static event EventHandler<EventArgs> ClipboardTextChanged;

    }
}
