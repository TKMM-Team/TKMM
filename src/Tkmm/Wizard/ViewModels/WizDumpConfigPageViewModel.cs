using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Tkmm.Wizard.ViewModels;

public sealed partial class WizDumpConfigPageViewModel : ObservableObject
{
    private static readonly FilePickerFileType _nsp = new("NSP") {
        Patterns = [
            "*.nsp"
        ]
    };

    private static readonly FilePickerFileType _xci = new("XCI") {
        Patterns = [
            "*.xci"
        ]
    };

    private static readonly FilePickerFileType _xciNsp = new("XCI/NSP") {
        Patterns = [
            "*.xci", "*.nsp"
        ]
    };

    [RelayCommand]
    private static async Task Browse(TextBox target)
    {
        string? result = target.Tag switch {
            "folder" => await BrowseFolder(),
            _ => await BrowseFile(target.Tag as string)
        };

        target.Text = result;
    }

    private static async ValueTask<string?> BrowseFolder()
    {
        return await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
            AllowMultiple = false
        }) switch {
            [IStorageFolder target] => target.TryGetLocalPath(),
            _ => null
        };
    }

    private static async ValueTask<string?> BrowseFile(string? types)
    {
        return await App.XamlRoot.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            AllowMultiple = false,
            FileTypeFilter = types switch {
                "xci,nsp" => [_xciNsp, _xci, _nsp],
                _ => [_nsp]
            }
        }) switch {
            [IStorageFile target] => target.TryGetLocalPath(),
            _ => null
        };
    }
}