using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.HighPerformance.Buffers;
using LibHac.Common.Keys;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using Microsoft.Extensions.Logging;
using TkSharp.Core;
using TkSharp.Core.IO.Parsers;
using TkSharp.Extensions.LibHac.Extensions;
using TkSharp.Extensions.LibHac.Util;
using Application = LibHac.Tools.Fs.Application;

namespace Tkmm.Core.Helpers;

public static class TkRomHelper
{
    public static IEnumerable<(string FilePath, string Version)> GetTotkRomFiles(IEnumerable<string> romFolderPaths, KeySet keys)
    {
        foreach (string target in romFolderPaths.SelectMany(dir => Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories))) {
            ReadOnlySpan<char> extension = Path.GetExtension(target.AsSpan());
            if (extension is not (".nsp" or ".xci")) {
                continue;
            }

            if (IsTotkRomFile(target, keys, out Application? app)) {
                yield return (FilePath: target, Version: app.DisplayVersion); 
            }
        }
    }

    public static bool IsTotkRomFile(string target, KeySet keys, [MaybeNullWhen(false)] out Application app)
    {
        if (!File.Exists(target)) {
            app = null;
            return false;
        }
        
        using LocalStorage storage = new(target, FileAccess.Read);
        using SwitchFs fs = storage.GetSwitchFs(target, keys);

        return fs.Applications.TryGetValue(TkGameRomUtils.EX_KING_APP_ID, out app);
    }

    public static KeySet? GetKeys(string keysFolder)
    {
        string prodKeysFilePath = Path.Combine(keysFolder, "prod.keys");
        if (!File.Exists(prodKeysFilePath)) {
            TkLog.Instance.LogError("A 'prod.keys' file could not be found in '{KeysFolder}'", keysFolder);
            return null;
        }
        
        string titleKeysFilePath = Path.Combine(keysFolder, "title.keys");
        if (!File.Exists(titleKeysFilePath)) {
            TkLog.Instance.LogError("A 'title.keys' file could not be found in '{KeysFolder}'", keysFolder);
            return null;
        }

        KeySet keys = new();
        ExternalKeyReader.ReadKeyFile(keys,
            prodKeysFilename: prodKeysFilePath,
            titleKeysFilename: titleKeysFilePath);

        return keys;
    }

    public static bool IsRomfsValid(string target)
    {
        return Directory.Exists(target) && GetVersionFromRomfs(target) is int version && version != 100;
    }

    public static int? GetVersionFromRomfs(string target)
    {
        string regionLangMaskFilePath = Path.Combine(target, "System", "RegionLangMask.txt");
        if (!File.Exists(regionLangMaskFilePath)) {
            return null;
        }

        using FileStream fs = File.OpenRead(regionLangMaskFilePath);
        int length = (int)fs.Length;
        using SpanOwner<byte> buffer = SpanOwner<byte>.Allocate(length);
        fs.ReadExactly(buffer.Span);
        
        return RegionLangMaskParser.ParseVersion(buffer.Span, out _);
    }
}