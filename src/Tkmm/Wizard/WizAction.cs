using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanguageExt;
using Tkmm.Wizard.ViewModels;

namespace Tkmm.Wizard;

public partial class WizAction(Either<TkLocale, object?> content, int defaultSelection, Func<ValueTask<(bool, int?)>>? onMoveNext = null) : ObservableObject
{
    private readonly int _defaultSelection = defaultSelection;
    
    [ObservableProperty]
    private object? _content = content.Match(
        obj => obj,
        key => Locale[key]
    );

    public Func<ValueTask<(bool, int?)>>? OnMoveNext { get; } = onMoveNext;

    [RelayCommand]
    private async Task MoveNext(WizPageViewModel currentPage)
    {
        if (OnMoveNext is null) {
            WizLayout.NextPage(currentPage, _defaultSelection);
            return;
        }
        
        (bool result, int? selection) = await OnMoveNext();
        
        if (result) {
            WizLayout.NextPage(currentPage, selection ?? _defaultSelection);
        }
    }
}