using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LibHac.Common.Keys;
using Microsoft.Extensions.Logging;
using TkSharp.Core;

namespace Tkmm.Core.Helpers;

// TODO: Translate error messages

public class RyujinxHelper
{
    public static KeySet? GetKeys(string ryujinxDataFolder, out string systemFolderPath)
    {
        systemFolderPath = Path.Combine(ryujinxDataFolder, "system");
        return TkRomHelper.GetKeys(systemFolderPath);
    }
    
    public static IEnumerable<(string FilePath, string Version)> GetTotkFiles(string ryujinxDataFolder, KeySet keys)
    {
        RyujinxConfig? config = GetRyujinxConfig(ryujinxDataFolder);

        if (config is null) {
            throw new InvalidOperationException("Ryujinx configuration could not be found.");
        }

        return TkRomHelper.GetTotkRomFiles(config.GameDirs, keys);
    }

    public static RyujinxConfig? GetRyujinxConfig(string ryujinxDataFolder)
    {
        string path = Path.Combine(ryujinxDataFolder, "Config.json");
        
        if (!File.Exists(path)) {
            TkLog.Instance.LogError("Ryujinx config file could not be found.");
            return null;
        }
        
        using FileStream fs = File.OpenRead(path);
        return JsonSerializer.Deserialize<RyujinxConfig>(fs);
    }

    public static string? GetRyujinxDataFolder(out string? ryujinxExeFilePath)
    {
        Process? ryujinx = Process
            .GetProcesses()
            .FirstOrDefault(x => x.ProcessName.Contains("Ryujinx", StringComparison.OrdinalIgnoreCase));

        ryujinxExeFilePath = ryujinx?.MainModule?.FileName;

        if (ryujinxExeFilePath is null) {
            return null;
        }

        if (Path.GetDirectoryName(ryujinxExeFilePath) is not string ryujinxFolder) {
            goto UseAppDataInstall;
        }
        
        string portable = Path.Combine(ryujinxFolder, "portable");
        
        if (Directory.Exists(portable)) {
            return portable;
        }
        
    UseAppDataInstall:
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ryujinx");
    }
}

public record RyujinxConfig(
    [property: JsonPropertyName("game_dirs")] List<string> GameDirs
);
