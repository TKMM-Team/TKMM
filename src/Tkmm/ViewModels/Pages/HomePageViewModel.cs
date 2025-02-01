#if RELEASE
using Avalonia.Threading;
#endif
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Tkmm.Actions;
using Tkmm.Core;
using Tkmm.Models;
using TkSharp;

namespace Tkmm.ViewModels.Pages;

public partial class HomePageViewModel : ObservableObject
{
    public static LayoutConfig Layout { get; } = LayoutConfig.Load("HomePageLayout");

    public static TkModManager ModManager => TKMM.ModManager;

    [RelayCommand]
    private static async Task ShowContributors()
    {
        ContentDialog dialog = new() {
            Title = "Contributors",
            Content = new TextBlock {
                Text = string.Join("\n", TKMM.ModManager.GetCurrentProfile().Selected?.Mod.Contributors
                    .Select(contributor => $"{contributor.Author}: {contributor.Contribution}") ?? []
                ),
                TextWrapping = TextWrapping.WrapWithOverflow
            },
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = "Dismiss"
        };

        await dialog.ShowAsync();
    }

    [RelayCommand]
    private static Task Merge()
    {
        return MergeActions.Instance.Merge();
    }

    [RelayCommand]
    private static Task Install()
    {
        return ImportActions.Instance.ImportFromFile();
    }

    [RelayCommand]
    private static void MoveUp()
    {
        TKMM.ModManager.GetCurrentProfile().MoveUp();
    }

    [RelayCommand]
    private static void MoveDown()
    {
        TKMM.ModManager.GetCurrentProfile().MoveDown();
    }

    [RelayCommand]
    private static Task Remove()
    {
        return ModActions.Instance.RemoveModFromProfile();
    }

    public HomePageViewModel()
    {
#if RELEASE
        _ = Dispatcher.UIThread.InvokeAsync(async () => await SystemActions.Instance.CheckForUpdates(isAutoCheck: false));
#endif
    }
}
