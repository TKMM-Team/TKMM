using Avalonia.Controls;

namespace Tkmm.Wizard.Views;

public partial class WizPage : UserControl
{
    public WizPage()
    {
        InitializeComponent();
        DataContext = WizLayout.FirstPage;
    }
}