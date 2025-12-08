using Stride.GameStudio.Helpers;

namespace Stride.GameStudio.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string EditorTitle { get; } = StrideGameStudio.EditorName;
}
