using System.Diagnostics;
using Avalonia.Controls.Notifications;
using Avalonia.Platform;
using Avalonia.Threading;
using AvaMark;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Humanizer;
using Microsoft.Extensions.Logging;
using Octokit;
using Tkmm.Core;
using Tkmm.Dialogs;
using Tkmm.Helpers;
using TkSharp.Core;

namespace Tkmm.Actions;

public sealed partial class SystemActions : GuardedActionGroup<SystemActions>
{
    protected override string ActionGroupName { get; } = nameof(SystemActions).Humanize();

    [RelayCommand]
    public static async Task ShowAboutDialog()
    {
        await using Stream aboutFileStream = AssetLoader.Open(new Uri("avares://Tkmm/Assets/About.md"));
        string contents = await new StreamReader(aboutFileStream).ReadToEndAsync();

        contents = contents.Replace("@@version@@", App.Version);

        TaskDialog dialog = new() {
            XamlRoot = App.XamlRoot,
            Title = "About",
            Content = new MarkdownViewer {
                Markdown = contents
            },
            Buttons = [
                TaskDialogButton.OKButton
            ]
        };

        await dialog.ShowAsync();
    }

    [RelayCommand]
    public static async Task OpenDocumentationWebsite()
    {
        try {
            Process.Start(new ProcessStartInfo("https://tkmm.org/docs/using-mods/") {
                UseShellExecute = true
            });
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "An error occured while trying to open the documentation website.");
            await ErrorDialog.ShowAsync(ex);
        }
    }

    [RelayCommand]
    public static Task CheckForUpdates(CancellationToken ct = default)
    {
        return CheckForUpdates(isAutoCheck: true, ct);
    }

    public static async Task CheckForUpdates(bool isAutoCheck = false, CancellationToken ct = default)
    {
        if (!OperatingSystem.IsWindows()) {
            if (isAutoCheck) return;

            await ApplicationUpdatesHelper.ShowUnsupportedPlatformDialog();
        }

        try {
            if (await ApplicationUpdatesHelper.HasAvailableUpdates() is Release release) {
                if (!isAutoCheck && !OperatingSystem.IsWindows()) {
                    await ApplicationUpdatesHelper.ShowUnsupportedPlatformDialog();
                    return;
                }
                else {
                    // TODO: Tell user there is an update before requesting a restart (they did not invoke the action)
                }
                
                await RequestUpdate(release, ct);
                return;
            }
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex,
                "An error occured while checking for application updates.");
            await ErrorDialog.ShowAsync(ex);
        }

        await new ContentDialog {
            Title = "Check for updates result",
            Content = "Software up to date.",
            PrimaryButtonText = "OK"
        }.ShowAsync();
    }

    public static Task RequestUpdate(Release release, CancellationToken ct = default)
    {
        return Dispatcher.UIThread.InvokeAsync(
            () => RequestUpdateInternal(release, ct));
    }

    private static async Task RequestUpdateInternal(Release release, CancellationToken ct = default)
    {
        if (!OperatingSystem.IsWindows()) {
            return;
        }
        
        ContentDialog dialog = new() {
            Title = "Update available. Proceed with update?",
            Content = "Your current session will be saved and closed, are you sure you wish to proceed?",
            PrimaryButtonText = "Yes",
            SecondaryButtonText = "Cancel"
        };

        if (await dialog.ShowAsync() is not ContentDialogResult.Primary) {
            return;
        }

        try {
            await ApplicationUpdatesHelper.PerformUpdates(release, ct);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex,
                "An error occured while updating the application.");
            await ErrorDialog.ShowAsync(ex);
        }
    }

    [RelayCommand]
    public static async Task CleanupTempFolder()
    {
        try {
            string tempFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".temp");

            if (!Directory.Exists(tempFolder)) {
                return;
            }

            Directory.Delete(tempFolder, recursive: true);
            Directory.CreateDirectory(tempFolder);

            App.Toast("The temporary folder was successfully deleted.",
                "Temporary Files Cleared", NotificationType.Success, TimeSpan.FromSeconds(3));
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "An error occured while trying to cleanup the temp folder.");
            await ErrorDialog.ShowAsync(ex);
        }
    }

    [RelayCommand]
    public static async Task SoftClose()
    {
        try {
            Config.Shared.Save();
            TKMM.ModManager.Save();
            Environment.Exit(0);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "An error occured while saving the mod manager state.");

            object errorReportResult = await ErrorDialog.ShowAsync(ex,
                TaskDialogStandardResult.Close, TaskDialogStandardResult.Cancel);
            if (errorReportResult is TaskDialogStandardResult.Close) {
                Environment.Exit(-1);
            }
        }
    }
}