// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Globalization;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using Stride.Core.Annotations;
using TextBox = Stride.Core.Presentation.Controls.TextBox;

namespace Stride.Core.Presentation.Behaviors
{
    /// <summary>
    /// This behavior allows more convenient editing of the value of a char using a TextBox.
    /// </summary>
    public class CharInputBehavior : Behavior<TextBox>
    {
        private bool updatingText;

        protected override void OnAttached()
        {
            AssociatedObject.TextChanged += TextChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.TextChanged -= TextChanged;
        }

        private void TextChanged(object sender, [NotNull] TextChangedEventArgs e)
        {
            if (updatingText)
                return;

            var text = AssociatedObject.Text;
            if (!string.IsNullOrEmpty(text))
            {
                updatingText = true;
                AssociatedObject.Text = text[^1].ToString(CultureInfo.InvariantCulture);
                updatingText = false;
            }
        }
    }
}
