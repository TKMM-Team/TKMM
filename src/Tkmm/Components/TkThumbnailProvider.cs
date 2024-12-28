using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using Tkmm.Core.Providers;
using TkSharp.Core;
using TkSharp.Core.Models;

namespace Tkmm.Components;

public sealed class TkThumbnailProvider(Bitmap defaultThumbnail) : ITkThumbnailProvider
{
    private static readonly HttpClient _client = new();
    
    public static readonly TkThumbnailProvider Instance;

    static TkThumbnailProvider()
    {
        using Stream defaultThumbnailBitmapStream = AssetLoader.Open(new Uri("avares://Tkmm/Assets/DefaultThumbnail.jpg"));
        Bitmap defaultThumbnailBitmap = new(defaultThumbnailBitmapStream);
        Instance = new TkThumbnailProvider(defaultThumbnailBitmap);
    }
    
    private readonly Bitmap _defaultThumbnail = defaultThumbnail;
    
    public async Task ResolveThumbnail(TkMod mod, CancellationToken ct = default)
    {
        TkThumbnail? thumbnail = mod.Thumbnail;
        ITkSystemSource? src = mod.Changelog.Source;
        
        if (thumbnail is null || src is null) {
            goto UseDefault;
        }

        if (!src.Exists(thumbnail.ThumbnailPath)) {
            goto TryUseUrl;
        }

        // ReSharper disable once ConvertToUsingDeclaration
        await using (Stream imageStream = src.OpenRead(thumbnail.ThumbnailPath)) {
            thumbnail.Bitmap = new Bitmap(imageStream);
            return;
        }
        
    TryUseUrl:
        if (!Uri.TryCreate(thumbnail.ThumbnailPath, UriKind.Absolute, out Uri? uri)) {
            goto UseDefault;
        }

        try {
            await using Stream imageStream = await _client.GetStreamAsync(uri, ct)
                .ConfigureAwait(false);
            
            using MemoryStream ms = new();
            await imageStream.CopyToAsync(ms, ct);
            ms.Seek(0, SeekOrigin.Begin);
            
            thumbnail.Bitmap = new Bitmap(ms);
            return;
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "Failed to resolve thumbnail URI '{Uri}'", uri);
        }
        
    UseDefault:
        mod.Thumbnail = new TkThumbnail {
            Bitmap = _defaultThumbnail,
            IsDefault = true
        };
    }
}