using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Tkmm.Core.Helpers;
using Tkmm.Core.Logging;
using Tkmm.Core.Providers;
using TkSharp;
using TkSharp.Core;
using TkSharp.Core.Models;
using TkSharp.Extensions.GameBanana.Readers;
using TkSharp.IO.Writers;
using TkSharp.Merging;

namespace Tkmm.Core;

// ReSharper disable once InconsistentNaming
public static class TKMM
{
    private static readonly Lazy<ITkRomProvider> _romProvider = new(() => new TkRomProvider());
    private static readonly TkModReaderProvider _readerProvider;
    private static ITkThumbnailProvider? _thumbnailProvider;

    public static readonly string MergedOutputFolder = Path.Combine(AppContext.BaseDirectory, ".merged");

    public static ITkRom Rom => _romProvider.Value.GetRom();

    public static Config Config => Config.Shared;

    public static TkModManager ModManager { get; }

    public static Task Initialize(ITkThumbnailProvider thumbnailProvider, CancellationToken ct = default)
    {
        _thumbnailProvider = thumbnailProvider;

        return Task.WhenAll(
            ModManager.Mods.Select(x => Task.Run(() => thumbnailProvider.ResolveThumbnail(x, ct), ct))
        );
    }

    public static async ValueTask<TkMod?> Install(object input, Stream? stream = null, TkModContext context = default, TkProfile? profile = null, CancellationToken ct = default)
    {
        if (_readerProvider.GetReader(input) is not ITkModReader reader) {
            TkLog.Instance.LogError("Could not locate mod reader for input: '{Input}'", input);
            return null;
        }

        if (await reader.ReadMod(input, stream, context, ct) is not TkMod mod) {
            TkLog.Instance.LogError("Failed to parse mod from input: '{Input}'", input);
            return null;
        }

        if (_thumbnailProvider?.ResolveThumbnail(mod, ct) is Task resolveThumbnail) {
            await resolveThumbnail;
        }

        ModManager.Import(mod, profile);
        return mod;
    }

    public static async ValueTask Merge(TkProfile profile, CancellationToken ct = default)
    {
        DirectoryHelper.DeleteTargetsFromDirectory(MergedOutputFolder, ["romfs", "exefs"], recursive: true);
        
        ITkModWriter writer = new FolderModWriter(MergedOutputFolder);

        TkMerger merger = new(writer, Rom);

        long startTime = Stopwatch.GetTimestamp();

        await merger.MergeAsync(TkModManager.GetMergeTargets(profile), ct);

        TimeSpan delta = Stopwatch.GetElapsedTime(startTime);
        TkLog.Instance.LogInformation("Elapsed time: {TotalMilliseconds}ms", delta.TotalMilliseconds);
    }

    static TKMM()
    {
        ModManager = TkModManager.CreatePortable();
        ModManager.CurrentProfile = ModManager.GetCurrentProfile();

        _readerProvider = new TkModReaderProvider(ModManager, _romProvider.Value);
        _readerProvider.Register(new GameBananaModReader(_readerProvider));
    }
}