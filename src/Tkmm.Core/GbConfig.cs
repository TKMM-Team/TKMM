using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;
using TkSharp.Extensions.GameBanana.Helpers;

namespace Tkmm.Core;

public sealed class GbConfig : ConfigModule<GbConfig>
{
    [Config(
        Header = "Use Threaded Downloads",
        Description = "Use multi-threaded downloads for potentially faster downloads. Disable this if you experience network issues.",
        Group = "Game Banana Client")]
    public bool UseThreadedDownloads {
        get => DownloadHelper.Config.UseThreadedDownloads;
        set {
            OnPropertyChanging();
            DownloadHelper.Config.UseThreadedDownloads = value;
            OnPropertyChanged();
        }
    }

    [Config(
        Header = "Download Timeout Seconds",
        Description = "The maximum amount of seconds to wait for a response before failing.",
        Group = "Game Banana Client")]
    public int GameBananaTimeoutSeconds {
        get => DownloadHelper.Config.TimeoutSeconds;
        set {
            OnPropertyChanging();
            DownloadHelper.Config.TimeoutSeconds = value;
            OnPropertyChanged();
        }
    }

    [Config(
        Header = "Game Banana Download Max Retries",
        Description = "The maximum amount of times to retry a download before failing.",
        Group = "Game Banana Client")]
    public int GameBananaMaxRetries {
        get => DownloadHelper.Config.MaxRetries;
        set {
            OnPropertyChanging();
            DownloadHelper.Config.MaxRetries = value;
            OnPropertyChanged();
        }
    }
}