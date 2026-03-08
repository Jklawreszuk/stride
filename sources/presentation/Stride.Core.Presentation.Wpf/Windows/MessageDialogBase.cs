// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Threading;
using Stride.Core.Presentation.Commands;
using Stride.Core.Presentation.Controls;
using Stride.Core.Presentation.View;
using Stride.Core.Presentation.ViewModels;

namespace Stride.Core.Presentation.Windows
{
    /// <summary>
    /// Base class for message-based dialog windows.
    /// </summary>
    public abstract class MessageDialogBase : ModalWindow
    {
        /// <summary>
        /// Identifies the <see cref="ButtonsSource"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<IEnumerable<DialogButtonInfo>> ButtonsSourceProperty =
            AvaloniaProperty.Register<MessageDialogBase, IEnumerable<DialogButtonInfo>>(nameof(ButtonsSource));

        /// <summary>
        /// Identifies the <see cref="MessageTemplate"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<DataTemplate> MessageTemplateProperty =
            AvaloniaProperty.Register<MessageDialogBase, DataTemplate>(nameof(MessageTemplate));

        /// <summary>
        /// Identifies the <see cref="MessageTemplateSelector"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<IDataTemplate> MessageTemplateSelectorProperty =
            AvaloniaProperty.Register<MessageDialogBase, IDataTemplate>(nameof(MessageTemplateSelector));
        
        /// <summary>
        /// Identifies the <see cref="ButtonCommand"/> dependency property.
        /// </summary>
        public static readonly DirectProperty<MessageDialogBase, ICommandBase> ButtonCommandProperty =
            AvaloniaProperty.RegisterDirect<MessageDialogBase, ICommandBase>(nameof(ButtonCommand), o => o.ButtonCommand);

        protected MessageDialogBase()
        {
            var serviceProvider = new ViewModelServiceProvider([new DispatcherService(Dispatcher.UIThread)]);
            ButtonCommand = new AnonymousCommand<int>(serviceProvider, ButtonClick);
        }

        public IEnumerable<DialogButtonInfo> ButtonsSource { get { return GetValue(ButtonsSourceProperty); } set { SetValue(ButtonsSourceProperty, value); } }

        public DataTemplate MessageTemplate { get { return GetValue(MessageTemplateProperty); } set { SetValue(MessageTemplateProperty, value); } }

        public IDataTemplate MessageTemplateSelector { get { return GetValue(MessageTemplateSelectorProperty); } set { SetValue(MessageTemplateSelectorProperty, value); }} 

        public int ButtonResult { get; private set; }

        protected ICommandBase ButtonCommand { get; }

        private void ButtonClick(int parameter)
        {
            ButtonResult = parameter;
            Close();
        }
    }

}
