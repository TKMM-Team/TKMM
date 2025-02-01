using Avalonia.Media;
using FluentAvalonia.UI.Windowing;
using Tkmm.Components;
using Tkmm.Core;
using Tkmm.Views.Common;
using PageManager = Tkmm.Components.PageManager;

namespace Tkmm.Models;

public class SplashScreen : IApplicationSplashScreen
{
    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public string? AppName { get; }

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public IImage? AppIcon { get; }

    public object SplashScreenContent { get; } = new SplashScreenView();

    public int MinimumShowTime => 1500;

    public async Task RunTasks(CancellationToken cancellationToken)
    {
        await TKMM.Initialize(TkThumbnailProvider.Instance, cancellationToken);
        
        PageManager.Shared.Focus(PageManager.Shared.Default);
    }
}