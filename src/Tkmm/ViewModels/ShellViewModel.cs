using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core;

namespace Tkmm.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    public static readonly ShellViewModel Shared = new();
    
    [ObservableProperty]
    private bool _isFirstTimeSetup = true;

    [ObservableProperty]
    private string _batteryIcon = string.Empty;

    [ObservableProperty]
    private int _batteryCharge = -1;
    
    public ShellViewModel()
    {
        IsFirstTimeSetup = !Config.Shared.ConfigExists();
    }
}