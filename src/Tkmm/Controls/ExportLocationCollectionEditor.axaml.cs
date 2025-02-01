using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using ConfigFactory.Avalonia.Helpers;
using ConfigFactory.Core.Attributes;
using Tkmm.Core.Models;

namespace Tkmm.Controls;

public partial class ExportLocationCollectionEditor : UserControl
{
    public static readonly ICommand BrowseCommand = new AsyncRelayCommand<ExportLocation>(Browse);

    public ExportLocationCollectionEditor()
    {
        InitializeComponent();
    }

    private static async Task Browse(ExportLocation? target)
    {
        BrowserDialog dialog = new(BrowserMode.OpenFolder, "Open Mod");
        if (target is not null && await dialog.ShowDialog() is string selectedFolder) {
            target.SymlinkPath = selectedFolder;
        }
    }
}
