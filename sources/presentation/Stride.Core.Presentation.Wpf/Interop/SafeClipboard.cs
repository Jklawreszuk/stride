// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Stride.Core.Annotations;
using Stride.Core.Extensions;

namespace Stride.Core.Presentation.Interop
{
    /// <summary>
    /// Wrapper around <see cref="Clipboard"/> that catches <see cref="COMException"/> related to clipboard errors. 
    /// </summary>
    public static class SafeClipboard
    {
        // ReSharper disable InconsistentNaming
        public const int CLIPBRD_E_CANT_OPEN = unchecked((int)0x800401D0);
        public const int CLIPBRD_E_CANT_SET = unchecked((int)0x800401D2);
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Similar to <see cref="Clipboard.ContainsText()"/> but don't throw if the clipboard cannot be open.
        /// </summary>
        /// <returns><c>true</c> if the Clipboard contains data in the <see cref="DataFormats.UnicodeText"/> data format; otherwise, <c>false</c>.</returns>
        public static async Task<bool> ContainsText()
        {
            var text = await GetText();//to verify
            return !text.IsNullOrEmpty();
        }

        /// <summary>
        /// Similar to <see cref="Clipboard.GetText()"/> but don't throw if the clipboard cannot be open.
        /// </summary>
        /// <returns>A string containing the <see cref="DataFormats.UnicodeText"/> data, or an empty string if no <see cref="DataFormats.UnicodeText"/> data is available on the Clipboard.</returns>
        [NotNull]
        public static async Task<string> GetText()
        {
            return await GetClipboard()?.TryGetTextAsync();
        }

        /// <summary>
        /// Similar to <see cref="Clipboard.SetDataObject(object, bool)"/> but don't throw if data cannot be set to the clipboard.
        /// </summary>
        /// <exception cref="ArgumentNullException">data is <c>null</c>.</exception>
        public static void SetDataObject([NotNull] IAsyncDataTransfer data, bool copy)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            GetClipboard()?.SetDataAsync(data);
        }

        /// <summary>
        /// Similar to <see cref="Clipboard.SetText(string)"/> but don't throw if data cannot be set to the clipboard.
        /// </summary>
        /// <exception cref="ArgumentNullException">data is <c>null</c>.</exception>
        public static void SetText([NotNull] string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            GetClipboard()?.SetTextAsync(text);
        }
        
        [CanBeNull]
        private static IClipboard GetClipboard()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow?.Clipboard;
            }

            throw new InvalidOperationException("Clipboard not available outside desktop lifetime.");
        }
    }
}
