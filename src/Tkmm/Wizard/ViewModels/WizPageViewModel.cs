using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanguageExt;

namespace Tkmm.Wizard.ViewModels;

public partial class WizPageViewModel(int id, WizPageViewModel? lastPage, TkLocale? title, Either<TkLocale, object?> pageContent, List<WizAction> actions) : ObservableObject
{
    
    [ObservableProperty]
    private int _id = id;

    public WizPageViewModel? LastPage { get; set; } = lastPage;
    
    [ObservableProperty]
    private string _title = title is null ? string.Empty : Locale[title.Value];
    
    [ObservableProperty]
    private object? _pageContent = pageContent.Match(
        obj => obj,
        key => Locale[key]
    );
    
    [ObservableProperty]
    private List<WizAction> _actions = actions;

    [RelayCommand]
    private void MoveBack()
    {
        if (LastPage is null) {
            return;
        }
        
        WizLayout.UpdatePage(this, LastPage);
    }
}