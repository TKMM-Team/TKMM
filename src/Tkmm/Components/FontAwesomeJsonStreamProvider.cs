using Avalonia.Platform;
using Projektanker.Icons.Avalonia.FontAwesome;

namespace Tkmm.Components;

public sealed class FontAwesomeJsonStreamProvider : IFontAwesomeUtf8JsonStreamProvider
{
    public static readonly FontAwesomeJsonStreamProvider Instance = new();

    public Stream GetUtf8JsonStream()
    {
        const string iconsResourcePath = "avares://Tkmm/Assets/fa-icons.json";
        Uri iconsResourceUri = new(iconsResourcePath);
        return AssetLoader.Open(iconsResourceUri);
    }
}
