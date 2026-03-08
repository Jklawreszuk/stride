// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;

namespace Stride.Core.Presentation.Drawing
{
    /// <summary>
    /// Provides a hosting <see cref="Control"/> for a collection of <see cref="Visual"/>.
    /// </summary>
    internal class VisualHost : Control
    {
        private readonly VisualCollection children;

        public VisualHost()
        {
            children = new VisualCollection(this);
        }
        
        /// <inheritdoc/>
        protected override int VisualChildrenCount => children.Count;

        public int AddChild(Visual child)
        {
            return children.Add(child);
        }

        public void AddChildren(IEnumerable<Visual> visuals)
        {
            foreach (var child in children)
            {
                children.Add(child);
            }
        }

        /// <inheritdoc/>
        protected override Visual GetVisualChild(int index)
        {
            return children[index];
        }
    }
}
