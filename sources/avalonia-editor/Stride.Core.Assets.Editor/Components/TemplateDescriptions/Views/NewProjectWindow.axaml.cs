using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Stride.Core.Assets.Editor.Components.TemplateDescriptions.ViewModels;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.IO;

namespace Stride.Core.Assets.Editor.Components.TemplateDescriptions.Views;

public partial class NewProjectWindow : Window
{
    public NewProjectWindow()
    {
        InitializeComponent();
    }
    
    public UDirectory DefaultOutputDirectory { get; set; }
        
    public NewPackageParameters Parameters { get; private set; }

    private NewProjectTemplateCollectionViewModel Templates => (NewProjectTemplateCollectionViewModel)DataContext;
}
