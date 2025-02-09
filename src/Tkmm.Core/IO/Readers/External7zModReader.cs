using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Tkmm.Core.Helpers;
using TkSharp.Core;
using TkSharp.Core.IO.ModSources;
using TkSharp.Core.Models;
using TkSharp.Merging;

namespace Tkmm.Core.IO.Readers;

// ReSharper disable once InconsistentNaming

public sealed class External7zModReader(ITkSystemProvider systemProvider, ITkRomProvider romProvider) : ITkModReader
{
    private readonly ITkSystemProvider _systemProvider = systemProvider;
    private readonly ITkRomProvider _romProvider = romProvider;
    
    public async ValueTask<TkMod?> ReadMod(object? input, Stream? stream = null, TkModContext context = default, CancellationToken ct = default)
    {
        if (input is not string fileName || stream is null) {
            TkLog.Instance.LogWarning(
                "[External 7z] Invalid input ('{Input}') with null stream", input);
            return null;
        }
        
        if (context.Id == Ulid.Empty) {
            context.Id = Ulid.NewUlid();
        }

        // Use a random ID instead of the
        // mod ID to avoid possible issues
        // if installing the same mod twice  
        string tmp = Path.Combine(Path.GetTempPath(), "tkmm", "7z", Ulid.NewUlid().ToString());
        Directory.CreateDirectory(tmp);
        
        string tmpInput = File.Exists(fileName) ? fileName : Path.Combine(tmp, "input");
        string tmpOutput = Path.Combine(tmp, "output");

        try {
            if (!File.Exists(tmpInput)) {
                await using FileStream fs = File.Create(tmpInput);
                await stream.CopyToAsync(fs, ct);
            }
            
            await External7zHelper.ExtractToFolder(tmpInput, tmpOutput, ct);

            if (!TryGetRoot(tmpOutput, out string? root)) {
                TkLog.Instance.LogWarning(
                    "[External 7z] Root folder could not be found when installing '{Input}'", input);
                return null;
            }
            
            FolderModSource source = new(root);
            ITkModWriter writer = _systemProvider.GetSystemWriter(context);

            using ITkRom rom = _romProvider.GetRom();
            TkChangelogBuilder builder = new(source, writer, _romProvider.GetRom(),
                _systemProvider.GetSystemSource(context.Id.ToString())
            );
            TkChangelog changelog = await builder.BuildAsync(ct);

            return new TkMod {
                Id = context.Id,
                Name = Path.GetFileNameWithoutExtension(fileName),
                Changelog = changelog
            };
        }
        finally {
            if (Directory.Exists(tmp)) {
                Directory.Delete(tmp, recursive: true);
            }
        }
    }

    public bool IsKnownInput(object? input)
    {
        return input is string path
               && Path.GetExtension(path.AsSpan()) is ".7z"
               && External7zHelper.CanUseExternal();
    }

    private static bool TryGetRoot(string tmp, [MaybeNullWhen(false)] out string root)
    {
        foreach (string directory in Directory.EnumerateDirectories(tmp, "*", SearchOption.AllDirectories)) {
            ReadOnlySpan<char> path = directory;
            
            if (path.Length <= 0) {
                continue;
            }

            ReadOnlySpan<char> normalized = directory[^1] is '\\' or '/' ? path[..^1] : path;
            switch (normalized.Length) {
                case > 4 when normalized[^5..] is "romfs" or "exefs":
                case > 5 when normalized[^6..] is "cheats":
                    root = Path.GetDirectoryName(directory);
                    return root is not null;
            }
        }
        
        root = null;
        return false;
    } 
}