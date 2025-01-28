using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Core.Models;

public sealed partial class ExportLocation : ObservableObject
{
    [ObservableProperty]
    private string _symlinkPath = string.Empty;

    [ObservableProperty]
    private bool _isEnabled = true;
}