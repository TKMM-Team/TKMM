using LibHac.Common.Keys;
using LibHac.Tools.Fs;
using Microsoft.Extensions.Logging;
using Tkmm.Core.Helpers;
using TkSharp.Core;
using TkSharp.Core.IO;
using TkSharp.Data.Embedded;
using TkSharp.Extensions.LibHac;

namespace Tkmm.Core.Providers;

public class TkRomProvider : ITkRomProvider
{
    public ITkRom GetRom()
    {
        TkChecksums checksums = TkChecksums.FromStream(TkEmbeddedDataSource.GetChecksumsBin());
        TkConfig tk = TkConfig.Shared;

        if (tk.GameDumpFolderPath is string gameDumpFolderPath && TkRomHelper.IsRomfsValid(gameDumpFolderPath)) {
            return new ExtractedTkRom(gameDumpFolderPath, checksums);
        }

        if (TkConfig.Shared.KeysFolderPath is not string keysFolderPath || TkRomHelper.GetKeys(keysFolderPath) is not KeySet keys) {
            throw new InvalidOperationException("Invalid keys folder.");
        }

        (LibHacRomSourceType? baseSource, string? basePath) = GetRomSource();
        if (baseSource is null || basePath is null) {
            throw new InvalidOperationException("Invalid configuration: Base game path is not set.");
        }

        if (baseSource == LibHacRomSourceType.File && 
            (TkConfig.Shared.BaseGameFilePath is not string baseGameFilePath ||
            !TkRomHelper.IsTotkRomFile(baseGameFilePath, keys, out Application? app) || app.DisplayVersion != "1.0.0")) {
            throw new InvalidOperationException("Invalid base game file.");
        }

        (LibHacRomSourceType? updateSource, string? updatePath) = GetUpdateSource();
        if (updateSource is null || updatePath is null) {
            throw new InvalidOperationException("Invalid configuration: Update path is not set.");
        }

        if (updateSource == LibHacRomSourceType.File && 
            (TkConfig.Shared.GameUpdateFilePath is not string gameUpdateFilePath ||
            !TkRomHelper.IsTotkRomFile(gameUpdateFilePath, keys, out Application? update) || update.DisplayVersion == "1.0.0")) {
            throw new InvalidOperationException("Invalid update file.");
        }

        var romProvider = new LibHacRomProvider();
        return romProvider.CreateRom(
            checksums,
            keys,
            baseSource.Value, basePath,
            updateSource.Value, updatePath);
    }

    private static (LibHacRomSourceType? Source, string? Path) GetRomSource()
    {
        TkConfig tk = TkConfig.Shared;

        if (tk.BaseGameFilePath is string path && File.Exists(path)) {
            return (LibHacRomSourceType.File, path);
        }
        
        if (tk.SplitFilesPath is string splitPath && Directory.Exists(splitPath)) {
            return (LibHacRomSourceType.SplitFiles, splitPath);
        }
        
        if (tk.SdCardRootPath is string sdPath && Directory.Exists(sdPath)) {
            return (LibHacRomSourceType.SdCard, sdPath);
        }
        
        return (null, null);
    }

    private static (LibHacRomSourceType? Source, string? Path) GetUpdateSource()
    {
        TkConfig tk = TkConfig.Shared;

        if (tk.GameUpdateFilePath is string path && File.Exists(path)) {
            return (LibHacRomSourceType.File, path);
        }

        if (tk.SdCardRootPath is string sdPath && Directory.Exists(sdPath)) {
            return (LibHacRomSourceType.SdCard, sdPath);
        }
        
        return (null, null);
    }

    /// <summary>
    /// Returns true if a rom can be provided with the current configurations. 
    /// </summary>
    public static bool CanProvideRom()
    {
        return CanProvideRom(message => TkLog.Instance.LogWarning("[Verify TotK Rom] {Message}", message));
    }

    public static bool CanProvideRom(Action<string> log)
    {
        if (!string.IsNullOrWhiteSpace(TkConfig.Shared.GameDumpFolderPath)) {
            if (TkRomHelper.GetVersionFromRomfs(TkConfig.Shared.GameDumpFolderPath) is not int version) {
                log("[RomFS Dump] Invalid game dump folder path.");
                goto CheckLibHac;
            }
            
            if (version == 100) {
                log("[RomFS Dump] Invalid game dump version (1.0.0), TKMM requires version 1.1.0 or later.");
            }
            return true;
        }
        
        CheckLibHac:
        if (TkConfig.Shared.KeysFolderPath is not string keysFolderPath || TkRomHelper.GetKeys(keysFolderPath) is not KeySet keys) {
            log("[Keys] The keys folder is not set or is invalid.");
            return false;
        }

        bool hasValidBaseGameFile = false;
        bool hasValidUpdateFile = false;
        bool hasValidSplitFiles = false;
        bool hasValidSdCard = false;

        if (TkConfig.Shared.BaseGameFilePath is string baseGameFilePath && !string.IsNullOrWhiteSpace(baseGameFilePath)) {
            if (!TkRomHelper.IsTotkRomFile(baseGameFilePath, keys, out Application? app) || app.DisplayVersion != "1.0.0") {
                log("[Base Game] The base game file is invalid.");
            }
            else {
                hasValidBaseGameFile = true;
            }
        }

        if (TkConfig.Shared.GameUpdateFilePath is string gameUpdateFilePath && !string.IsNullOrWhiteSpace(gameUpdateFilePath)) {
            if (!TkRomHelper.IsTotkRomFile(gameUpdateFilePath, keys, out Application? update) || update.DisplayVersion == "1.0.0") {
                log("[Update] The game update file is invalid.");
            }
            else {
                hasValidUpdateFile = true;
            }
        }

        if (TkConfig.Shared.SplitFilesPath is string splitPath && Directory.Exists(splitPath)) {
            hasValidSplitFiles = true;
        }

        if (!string.IsNullOrWhiteSpace(TkConfig.Shared.SdCardRootPath)) {
            string nintendoContentsPath = Path.Combine(TkConfig.Shared.SdCardRootPath, "Nintendo", "Contents");
            if (!Directory.Exists(nintendoContentsPath)) {
                log("[SD Card] The Nintendo/Contents folder is not present in the SD card path.");
            }
            else {
                hasValidSdCard = true;
            }
        }

        bool hasValidBaseSource = hasValidBaseGameFile || hasValidSplitFiles || hasValidSdCard;
        bool hasValidUpdateSource = hasValidUpdateFile || hasValidSdCard;

        if (!hasValidBaseSource) {
            log("[Base Game] No valid base game source found (file, split files, or SD card).");
            return false;
        }

        if (!hasValidUpdateSource) {
            log("[Update] No valid update source found (file or SD card).");
            return false;
        }

        return true;
    }
}