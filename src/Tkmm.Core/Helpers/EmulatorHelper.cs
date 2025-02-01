using Humanizer;
using LibHac.Common.Keys;
using Microsoft.Extensions.Logging;
using TkSharp.Core;

namespace Tkmm.Core.Helpers;

public class EmulatorHelper
{
    public static KeySet? GetKeys(string emulatorDataFolder, out string keysFolderPath)
    {
        keysFolderPath = Path.Combine(emulatorDataFolder, "keys");
        return TkRomHelper.GetKeys(keysFolderPath);
    }
    
    public static IEnumerable<(string FilePath, string Version)> GetTotkFiles(string emulatorName, string emulatorDataFolder, KeySet keys)
    {
        string qtConfigFilePath = Path.Combine(emulatorDataFolder, "config", "qt-config.ini");
        
        if (!File.Exists(qtConfigFilePath)) {
            TkLog.Instance.LogError("{EmulatorName} configuration could not be found.", emulatorName.Humanize(LetterCasing.Title));
            return [];
        }

        return TkRomHelper.GetTotkRomFiles(GetGameFolderPaths(qtConfigFilePath), keys);
    }

    private static List<string> GetGameFolderPaths(string qtConfigFilePath)
    {
        using FileStream fs = File.OpenRead(qtConfigFilePath);
        using StreamReader reader = new(fs);
        
        List<string> results = [];

        Span<Range> ranges = stackalloc Range[2];
        ref Range itemKey = ref ranges[0];
        ref Range itemValue = ref ranges[1];
        
        while (reader.ReadLine() is string line) {
            ReadOnlySpan<char> item = line.AsSpan();

            if (item.Length < 24 || item[..15] is not @"Paths\gamedirs\") {
                continue;
            }

            if (item.Split(ranges, '=') < 2 || item[itemKey][^4..] is not "path") {
                continue;
            }
            
            string romFolderPath = line[itemValue];

            if (!Directory.Exists(romFolderPath)) {
                continue;
            }
            
            results.Add(romFolderPath);
        }

        return results;
    }
}