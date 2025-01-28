using Avalonia.Controls;
using Tkmm.Wizard.Actions;

namespace Tkmm.Wizard.Views;

public partial class WizEmulatorSelectionPage : UserControl
{
    public static readonly WizEmulatorSelectionPage Instance = new();
    
    public WizEmulatorSelectionPage()
    {
        InitializeComponent();
    }

    public async ValueTask<(bool, int?)> CheckSelection()
    {
        return (RyujinxOption.IsChecked, SwitchOption.IsChecked, OtherOption.IsChecked) switch {
            (true, false, false) => (true, 0),
            (false, true, false) => (true, null),
            (false, false, true) => await WizActions.SetupOtherEmulator(),
            _ => (false, null),
        };
    }
}