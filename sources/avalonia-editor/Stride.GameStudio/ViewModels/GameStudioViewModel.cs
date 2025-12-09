using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.MostRecentlyUsedFiles;
using Stride.Core.Presentation.Collections;
using Stride.Core.Presentation.Commands;
using Stride.GameStudio.Helpers;

namespace Stride.GameStudio.ViewModels;

public partial class GameStudioViewModel : EditorViewModel
{
    public string EditorTitle { get; } = StrideGameStudio.EditorName;
    public ICommandBase NewSessionCommand { get; }
    public ICommandBase OpenSessionCommand { get; }
    public ObservableList<MostRecentlyUsedFile> RecentFiles { get; } = [];
    public object FilePath { get; }
}
