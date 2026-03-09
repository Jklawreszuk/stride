// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using Stride.Core.Annotations;
using Stride.Core.Extensions;

namespace Stride.Core.Presentation.Controls.Commands
{
    /// <summary>
    /// This class provides an instance of all commands in the namespace <see cref="Commands"/>.
    /// These instances can be used in XAML with the <see cref="System.Windows.Markup.StaticExtension"/> markup extension.
    /// </summary>
    public static class ControlCommands
    {
        /// <summary>
        /// Initialize the static properties of the <see cref="ControlCommands"/> class.
        /// </summary>
        static ControlCommands()
        {
            ClearSelectionCommand = new RelayCommand<Control>(OnClearSelectionCommand);
            SetAllVectorComponentsCommand = new RelayCommand<VectorEditorBase>(OnSetAllVectorComponents);
            ResetValueCommand = new RelayCommand<VectorEditorBase>(OnResetValue);
        }

        /// <summary>
        /// Clears the current selection of a text box.
        /// </summary>
        [NotNull]
        public static ICommand ClearSelectionCommand { get; }

        /// <summary>
        /// Sets all the components of a <see cref="VectorEditorBase"/> to the value given as parameter.
        /// </summary>
        [NotNull]
        public static ICommand SetAllVectorComponentsCommand { get; }

        /// <summary>
        /// Resets the current value of a vector editor to the value set in the <see cref="VectorEditorBase{T}.DefaultValue"/> property.
        /// </summary>
        [NotNull]
        public static ICommand ResetValueCommand { get; }
        
        private static void OnClearSelectionCommand(Control control)
        {
            if (control is not null)
            {
                switch (control)
                {
                    case ListBox listBox:
                        listBox.SelectedItem = null;
                        break;
                    case ComboBox comboBox:
                        comboBox.SelectedItem = null;
                        break;
                    case TabControl tabControl:
                        tabControl.SelectedIndex = -1;
                        break;
                }
            }
        }

        private static void OnSetAllVectorComponents(VectorEditorBase vectorEditor)
        {
            if (vectorEditor is not null)
            {
                try
                {
                    var value = Convert.ToSingle(e.Parameter);
                    vectorEditor.SetVectorFromValue(value);
                }
                catch (Exception ex)
                {
                    ex.Ignore();
                }
            }
        }

        private static void OnResetValue(VectorEditorBase vectorEditor)
        {
            vectorEditor?.ResetValue();
        }
    }
}
