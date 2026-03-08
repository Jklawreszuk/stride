// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Themes;

namespace Stride.Core.Presentation.MarkupExtensions
{
    using static Stride.Core.Presentation.Themes.IconThemeSelector;

    public class ThemedSourceExtension : MarkupExtension
    {
        public ThemedSourceExtension() { }

        public ThemedSourceExtension(IImage source, ThemeBase theme)
        {
            Source = source;
            Theme = theme.GetIconTheme();
        }

        [ConstructorArgument("source")]
        private IImage Source { get; }

        [ConstructorArgument("theme")]
        private IconTheme Theme { get; }

        [NotNull]
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Source is DrawingImage drawingImage)
            {
                return new DrawingImage
                {
                    Drawing = ImageThemingUtilities.TransformDrawing(drawingImage.Drawing, Theme)
                };
            }

            return Source;
        }
    }
}
