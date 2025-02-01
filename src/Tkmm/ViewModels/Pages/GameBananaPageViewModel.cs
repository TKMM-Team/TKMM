using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
using Tkmm.Actions;
using Tkmm.Core;
using Tkmm.Views.Common;
using TkSharp.Core;
using TkSharp.Extensions.GameBanana;
using TkSharp.Extensions.GameBanana.Helpers;
using TkSharp.Extensions.GameBanana.Models;

namespace Tkmm.ViewModels.Pages;

public partial class GameBananaPageViewModel : ObservableObject
{
    private const int GAME_ID = 7617;

    private static readonly GameBananaFeed? _suggestedModsFeed = GetSuggestedFeed();

    [ObservableProperty]
    private string _searchArgument = string.Empty;

    [ObservableProperty]
    private bool _isShowingSuggested;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private double? _loadProgress;

    [ObservableProperty]
    private bool _isLoadSuccess;

    [ObservableProperty]
    private IGameBananaSource _source = new GameBananaSource(GAME_ID);

    [ObservableProperty]
    private double? _downloadSpeed;

    public GameBananaPageViewModel()
    {
        DownloadHelper.Reporter = new DownloadReporter {
            ProgressReporter = new Progress<double>(
                progress => {
                    if (IsLoading) LoadProgress = progress;
                }
            ),
            SpeedReporter = new Progress<double>(
                speed => {
                    if (IsLoading) DownloadSpeed = speed;
                })
        };
        
        DownloadHelper.OnDownloadStarted += () => {
            IsLoading = true;
            
            return Task.CompletedTask;
        };

        DownloadHelper.OnDownloadCompleted += () => {
            IsLoading = false;
            IsLoadSuccess = true;
            LoadProgress = 0;
            DownloadSpeed = null;
            
            return Task.CompletedTask;
        };

        _ = Refresh();

        TKMM.Config.PropertyChanged += async (_, eventArgs) => {
            if (eventArgs.PropertyName is not nameof(Config.GameBananaSortMode)) {
                return;
            }

            Source.SortMode = TKMM.Config.GameBananaSortMode;
            await Refresh();
            Config.Shared.Save();
        };
    }
    
    [RelayCommand]
    public async Task Refresh(ScrollViewer? modsViewer = null)
    {
        await ReloadPage(
            IsShowingSuggested ? _suggestedModsFeed : null
        );
        
        modsViewer?.ScrollToHome();
    }

    [RelayCommand]
    public async Task Search(ScrollViewer modsViewer)
    {
        Source.CurrentPage = 0;
        await Refresh(modsViewer);
    }

    [RelayCommand]
    public async Task ResetSearch(ScrollViewer modsViewer)
    {
        Source.CurrentPage = 0;
        SearchArgument = string.Empty;
        await Refresh(modsViewer);
    }

    [RelayCommand]
    public async Task NextPage(ScrollViewer modsViewer)
    {
        Source.CurrentPage++;
        await Refresh(modsViewer);
    }

    [RelayCommand]
    public async Task PrevPage(ScrollViewer modsViewer)
    {
        Source.CurrentPage--;
        await Refresh(modsViewer);
    }

    [RelayCommand(CanExecute = nameof(CanShowSuggested))]
    public async Task ShowSuggested(ScrollViewer modsViewer)
    {
        if (IsShowingSuggested) {
            await ReloadPage(_suggestedModsFeed);
            modsViewer.ScrollToHome();
            return;
        }

        await ReloadPage();
    }

    private static bool CanShowSuggested()
    {
        return _suggestedModsFeed is not null;
    }

    [RelayCommand]
    public static async Task InstallMod(GameBananaModRecord mod)
    {
        ArgumentNullException.ThrowIfNull(mod.Full, nameof(mod.Full));

        GameBananaInstallPreview preview = new() {
            DataContext = mod
        };

        GameBananaFile? target = mod.Full.Files
            .FirstOrDefault(file => file.IsSelected);

        ContentDialog dialog = new() {
            Title = $"Install {mod.Name}",
            Content = preview,
            SecondaryButtonText = "Cancel",
            PrimaryButtonText = "Install",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = target is not null,
        };

        foreach (GameBananaFile file in mod.Full.Files) {
            file.PropertyChanged += (_, eventArgs) => {
                if (eventArgs.PropertyName == nameof(file.IsSelected)) {
                    target = file;
                    dialog.IsPrimaryButtonEnabled = true;
                }
            };
        }

        if (await dialog.ShowAsync() is not ContentDialogResult.Primary || target is null) {
            return;
        }
        
        TkStatus.Set($"Downloading '{target.Name}'", "fa-solid fa-download", StatusType.Working);
        await ModActions.Instance.Install((mod.Full, target));
    }

    private async Task ReloadPage(GameBananaFeed? customFeed = null)
    {
        Source.Feed?.Records.Clear();
        IsLoadSuccess = IsLoading = true;

        try {
            await Source.LoadPage(Source.CurrentPage, SearchArgument, customFeed);
            IsLoadSuccess = true;
        }
        catch (Exception ex) {
            IsLoadSuccess = false;
            TkLog.Instance.LogError(ex,
                "An error occured when reloading page {CurrentPage} with search {SearchArgument}",
                Source.CurrentPage, SearchArgument);
            App.ToastError(ex);
        }
        finally {
            IsLoading = false;
        }
    }

    private static GameBananaFeed? GetSuggestedFeed()
    {
        try {
            using Stream stream = AssetLoader.Open(new Uri("avares://Tkmm/Assets/GameBanana/Suggested.json"));
            return JsonSerializer.Deserialize(stream, GameBananaFeedJsonContext.Default.GameBananaFeed);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex,
                "An error occured when reading the suggested mods list. The feature will be disabled until restarting.");
            App.ToastError(ex);
        }
        
        return null;
    }
}