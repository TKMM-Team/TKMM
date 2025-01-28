using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Models;

public partial class NamedDialog(string name, bool isSuppressed) : ObservableObject
{
    [ObservableProperty]
    private string _name = name;
    
    [ObservableProperty]
    private bool _isSuppressed = isSuppressed;
}