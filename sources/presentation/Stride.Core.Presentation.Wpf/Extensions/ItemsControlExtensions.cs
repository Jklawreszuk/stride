// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Stride.Core.Annotations;

namespace Stride.Core.Presentation.Extensions
{
    public static class ItemsControlExtensions
    {
        [CanBeNull]
        public static ItemsControl GetParentContainer([NotNull] this ItemsControl itemsControl)
        {
            var parent = itemsControl.GetVisualParent();

            while (parent != null && parent is not ItemsControl)
                parent = parent.GetVisualParent();

            return parent as ItemsControl;
        }

        public static IEnumerable<ItemsControl> GetChildContainers([NotNull] this ItemsControl itemsControl)
        {
            foreach (var item in itemsControl.Items)
            {
                if (itemsControl.ContainerFromItem(item) is ItemsControl container)
                    yield return container;
            }
        }
    }
}
