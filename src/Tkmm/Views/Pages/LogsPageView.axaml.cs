using Avalonia.Controls;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;
public partial class LogsPageView : UserControl
{
    public LogsPageView()
    {
        InitializeComponent();
        DataContext = new LogsPageViewModel();
    }
}
