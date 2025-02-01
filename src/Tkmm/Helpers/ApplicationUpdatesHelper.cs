using System.IO.Compression;
using System.Runtime.Versioning;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
using Octokit;
using Tkmm.Core.Helpers;
using TkSharp.Core;

namespace Tkmm.Helpers;

public static class ApplicationUpdatesHelper
{
    public static async ValueTask<Release?> HasAvailableUpdates()
    {
        Release latest = await OctokitHelper.GetLatestRelease("TKMM-Team", "Tkmm");
        return latest.TagName != App.Version
            ? latest : null;
    }

    [SupportedOSPlatform("Windows")]
    public static async ValueTask PerformUpdates(Release release, CancellationToken ct = default)
    {
        await using Stream? stream = await OctokitHelper.DownloadReleaseAsset(release);
        
        if (stream is null) {
            TkLog.Instance.LogError(
                "Update failed: Could not locate and/or download release assets from '{Tag}'.",
                release.TagName);
            return;
        }

        string output = Path.Combine(AppContext.BaseDirectory, ".update");
        if (Directory.Exists(output)) {
            Directory.Delete(output, recursive: true);
        }
        
        ZipArchive archive = new(stream, ZipArchiveMode.Read);
        archive.ExtractToDirectory(output, overwriteFiles: true);
        
        // TODO: Update
    }

    public static async ValueTask ShowUnsupportedPlatformDialog()
    {
        await new ContentDialog {
            Title = "Unsupported operating system",
            Content = "In-app updates are only supported on Windows. Use the app store or your package manager to update.",
            PrimaryButtonText = "OK"
        }.ShowAsync();
    }
}