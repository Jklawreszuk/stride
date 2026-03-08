// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using Avalonia.Controls;
using Stride.Core.Annotations;

namespace Stride.Core.Presentation.Windows
{
    /// <summary>
    /// A container object for windows and their related information.
    /// </summary>
    public class WindowInfo : IEquatable<WindowInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowInfo"/> class.
        /// </summary>
        /// <param name="window">The window represented by this object.</param>
        public WindowInfo([NotNull] Window window)
        {
            Window = window ?? throw new ArgumentNullException(nameof(window));
        }
        
        /// <summary>
        /// Gets the <see cref="Window"/> represented by this object, if available.
        /// </summary>
        public Window Window { get; }

        /// <summary>
        /// Gets whether the corresponding window is currently disabled.
        /// </summary>
        public bool IsDisabled { get => !Window.IsEnabled; internal set => Window.IsEnabled = value; }

        /// <summary>
        /// Gets whether the corresponding window is currently shown.
        /// </summary>
        public bool IsShown
        {
            get;
            internal set
            {
                field = value;
            }
        }

        /// <summary>
        /// Gets whether the corresponding window is a blocking window.
        /// </summary>
        public bool IsBlocking { get; internal set; }

        /// <summary>
        /// Gets whether the corresponding window is currently modal.
        /// </summary>
        /// <remarks>
        /// This methods is heuristic, since there is no absolute flag under Windows indicating whether
        /// a window is modal. This method might need to be adjusted depending on the use cases.
        /// </remarks>
        public bool IsModal
        {
            get
            {
                if (IsBlocking)
                    return false;

                var owner = Owner;
                return owner == null || owner.Window.IsDialog && !owner.Window.IsEnabled;
            }
        }

        /// <summary>
        /// Gets the owner of this window.
        /// </summary>
        [CanBeNull]
        public WindowInfo Owner
        {
            get
            {
                if (!IsShown)
                    return null;

                return Window.Owner is Window owner ? (WindowManager.Find(owner) ?? new WindowInfo(owner)) : null;
            }
            internal set;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as WindowInfo);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Window?.GetHashCode() ?? 0;
        }

        /// <inheritdoc/>
        public static bool operator ==(WindowInfo left, WindowInfo right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc/>
        public static bool operator !=(WindowInfo left, WindowInfo right)
        {
            return !Equals(left, right);
        }

        /// <inheritdoc/>
        public bool Equals(WindowInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Window, other.Window);
        }
    }
}
