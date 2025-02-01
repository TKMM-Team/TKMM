using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Core.Models.NX;

public sealed partial class NxNetworkService(string name, Action<bool> onEnabledChanged) : ObservableObject
{
    [ObservableProperty]
    private string _name = name;

    [ObservableProperty]
    private bool _isEnabled;

    public Action<bool> OnEnabledChanged { get; } = onEnabledChanged;

    partial void OnIsEnabledChanged(bool value)
    {
        OnEnabledChanged(value);
    }
}