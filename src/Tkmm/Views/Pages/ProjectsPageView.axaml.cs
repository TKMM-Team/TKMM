using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Tkmm.ViewModels.Pages;
using TkSharp.Packaging;

namespace Tkmm.Views.Pages;

public partial class ProjectsPageView : UserControl
{
    public ProjectsPageView()
    {
        InitializeComponent();
    }

    private void InputElement_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not ProjectsPageViewModel vm) {
            return;
        }
        
        if (e.Source is ContentPresenter { Content: TkProject project }) {
            vm.Project = TkProjectManager.OpenProject(project.FolderPath);
        }
    }
}