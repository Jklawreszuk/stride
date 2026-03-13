// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;

namespace Stride.Core.Presentation.Themes
{
    public class ThemeResourceDictionary : ResourceDictionary
    {    
        private ResourceInclude? _currentInclude;
        // New themes are added here as new properties.

        public Uri ExpressionDarkSource
        {
            get;
            set => SetValue(ref field, value);
        }

        public Uri DarkSteelSource
        {
            get;
            set => SetValue(ref field, value);
        }

        public Uri DividedSource
        {
            get;
            set => SetValue(ref field, value);
        }

        public Uri LightSteelBlueSource
        {
            get;
            set => SetValue(ref field, value);
        }

        public void UpdateSource(ThemeType themeType)
        {
            Uri? uri = themeType switch
            {
                ThemeType.ExpressionDark => ExpressionDarkSource,
                ThemeType.DarkSteel => DarkSteelSource,
                ThemeType.Divided => DividedSource,
                ThemeType.LightSteelBlue => LightSteelBlueSource,
                _ => null
            };

            if (uri == null)
                return;

            if (_currentInclude != null)
                MergedDictionaries.Remove(_currentInclude);

            _currentInclude = new ResourceInclude(uri) { Source = uri };
            MergedDictionaries.Add(_currentInclude);
        }

        private void SetValue(ref Uri sourceBackingField, Uri value)
        {
            sourceBackingField = value;
            UpdateSource(ThemeController.CurrentTheme);
        }
    }
}
