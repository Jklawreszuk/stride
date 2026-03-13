// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Stride.Core.Presentation.Interop;
using Stride.Core.Annotations;
using Stride.Core.Presentation.Services;

namespace Stride.Core.Presentation.Windows
{
    using MessageBoxImage = Services.MessageBoxImage;

    public class MessageBox : MessageDialogBase
    {
        /// <summary>
        /// Identifies the <see cref="Image"/> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty ImageProperty =
            AvaloniaProperty.Register<MessageBox, IImage>(nameof(Image));

        protected MessageBox()
        {
            Initialized += OnInitialized;
        }

        protected void OnInitialized(object sender, EventArgs e)
        {
            KeyBindings.Add(new KeyBinding
            {
                Gesture = new KeyGesture(Key.C, KeyModifiers.Control),
                Command = new RelayCommand(() =>
                    SafeClipboard.SetDataObject(Content ?? string.Empty, true))
            });
        }

        public IImage Image
        {
            get { return (IImage)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        internal static void SetImage([NotNull] MessageBox messageBox, MessageBoxImage image)
        {
            string imageKey = image switch
            {
                MessageBoxImage.None => null,
                MessageBoxImage.Error => "ImageErrorDialog",
                MessageBoxImage.Question => "ImageQuestionDialog",
                MessageBoxImage.Warning => "ImageWarningDialog",
                MessageBoxImage.Information => "ImageInformationDialog",
                _ => throw new ArgumentOutOfRangeException(nameof(image), image, null)
            };
            if (imageKey != null && messageBox.TryFindResource(imageKey, out var result))
                messageBox.Image = (IImage)result;
        }

        /// <summary>
        /// Displays a <see cref="MessageBox"/> an returns the <see cref="MessageBoxResult"/> depending on the user's choice.
        /// </summary>
        /// <param name="message">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="buttons">An enumeration of <see cref="DialogButtonInfo"/> that specifies buttons to display</param>
        /// <param name="image">A <see cref="MessageBoxImage"/> value that specifies the icon to display.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        public static async Task<int> Show(string message, string caption, [NotNull] IEnumerable<DialogButtonInfo> buttons, MessageBoxImage image)
        {
            var buttonList = buttons.ToList();
            var messageBox = new MessageBox
            {
                Title = caption,
                Content = message,
                ButtonsSource = buttonList,
            };
            SetImage(messageBox, image);
            SetKeyBindings(messageBox, buttonList);
            await messageBox.ShowModal();
            return messageBox.ButtonResult;
        }

        internal static void SetKeyBindings(MessageBox messageBox, [NotNull] IEnumerable<DialogButtonInfo> buttons)
        {
            foreach (var button in buttons)
            {
                if (!Enum.TryParse(button.Key, out Key key))
                    continue;

                var binding = new KeyBinding()
                {
                    CommandParameter = button.Result,
                    Command = messageBox.ButtonCommand,
                    Gesture = new KeyGesture(key, KeyModifiers.Alt)
                };
                messageBox.KeyBindings.Add(binding);
            }
        }
    }
}
