using TkSharp.Core.Models;

namespace Tkmm.Core.Providers;

public interface ITkThumbnailProvider
{
    Task ResolveThumbnail(TkMod mod, CancellationToken ct = default);
}