using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace Tkmm.Core.Models;

public sealed partial class ExportLocations : ObservableCollection<ExportLocation>
{
    [RelayCommand]
    private void New()
    {
        Add(new ExportLocation());
    }

    [RelayCommand]
    private void Delete(ExportLocation target)
    {
        Remove(target);
    }
}