using System.Collections.ObjectModel;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Core.Logging;
using Tkmm.Core.Models;

namespace Tkmm.ViewModels.Pages;

public partial class LogsPageViewModel : ObservableObject
{
    public static ObservableCollection<EventLog> Logs => EventLogger.Logs;

    [ObservableProperty]
    private EventLog? _selected;

    [RelayCommand]
    private void Copy()
    {
        if (Selected is null) {
            return;
        }

        Copy(Selected.ToString());
    }

    [RelayCommand]
    private void CopyMarkdown()
    {
        if (Selected is null) {
            return;
        }

        Copy(Selected.ToMarkdown());
    }

    private static void Copy(string text)
    {
        if (App.XamlRoot.Clipboard is IClipboard clipboard) {
            clipboard.SetTextAsync(text);
        }
    }
}
