// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;

namespace Stride.Core.Presentation.Controls
{
    public class TextBlockFormatting
    {
        static TextBlockFormatting()
        {
            FormattedTextProperty.Changed.AddClassHandler<AvaloniaObject>(OnFormattedTextChanged);
        }
        
        public static Inline GetFormattedText(AvaloniaObject obj)
        {
            return (Inline)obj.GetValue(FormattedTextProperty);
        }

        public static void SetFormattedText(AvaloniaObject obj, Inline value)
        {
            obj.SetValue(FormattedTextProperty, value);
        }

        /// <summary>
        /// Identifies the dependency property which permits to directly bind a inline to a <see cref="TextBox"/>.
        /// </summary>
        public static readonly AvaloniaProperty FormattedTextProperty =
            AvaloniaProperty.RegisterAttached<TextBlockFormatting, Control, Inline>("FormattedText");

        private static void OnFormattedTextChanged(AvaloniaObject o, AvaloniaPropertyChangedEventArgs e)
        {
            if (o is not TextBlock textBlock) return;

            var inline = (Inline)e.NewValue;
            if (inline == null)
            {
                textBlock.Inlines.Clear();
            }
            else
            {
                textBlock.Inlines.Add(inline);
            }
        }
    }
}
