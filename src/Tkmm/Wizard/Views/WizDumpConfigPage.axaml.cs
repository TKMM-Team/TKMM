using Avalonia.Controls;
using WizDumpConfigPageViewModel = Tkmm.Wizard.ViewModels.WizDumpConfigPageViewModel;

namespace Tkmm.Wizard.Views;

public partial class WizDumpConfigPage : UserControl
{
    public WizDumpConfigPageViewModel Vm { get; } = new();
    
    public WizDumpConfigPage()
    {
        InitializeComponent();
        DataContext = Vm;
    }
}