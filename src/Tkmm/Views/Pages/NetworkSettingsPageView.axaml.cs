using Avalonia.Controls;
using Avalonia.Input;
using Tkmm.Core.Models.NX;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;

public partial class NetworkSettingsPageView : UserControl
{
    public NetworkSettingsPageView()
    {
        InitializeComponent();
        DataContext = new NetworkSettingsPageViewModel();
    }

    private void PasswordBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter || sender is not TextBox { DataContext: NxNetwork network }) {
            return;
        }
        
        network.ConnectCommand.Execute(parameter: null);
    }
}