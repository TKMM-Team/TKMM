using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm;

public enum StatusType
{
    Static,
    Working
}

public sealed partial class TkStatus : ObservableObject
{
    public static TkStatus Shared { get; } = new();

    private TkStatus()
    {
        _timer = new Timer(UpdateLoadingStatus);
        _timer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0.3));
    }

    private const string DOT = ".";
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Timer _timer;

    [ObservableProperty]
    private string _status = Locale[TkLocale.Status_Ready];

    [ObservableProperty]
    private string _suffix = string.Empty;

    [ObservableProperty]
    private string _icon = TkIcons.READY;

    [ObservableProperty]
    private StatusType _type = StatusType.Static;

    /// <summary>
    /// Reset the status.
    /// </summary>
    public static void Reset() => Shared.SetInternal(Locale[TkLocale.Status_Ready]);

    /// <summary>
    /// Set a temporary status for 1.5 seconds.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="icon"></param>
    /// <param name="type"></param>
    public static void SetTemporaryShort(string status, string icon = TkIcons.READY, StatusType type = StatusType.Static)
        => Shared.SetTemporaryInternal(status, icon, type, duration: 1.5);

    /// <summary>
    /// Set a temporary status for 3 seconds.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="icon"></param>
    /// <param name="type"></param>
    public static void SetTemporaryLong(string status, string icon = TkIcons.READY, StatusType type = StatusType.Static)
        => Shared.SetTemporaryInternal(status, icon, type, duration: 3);

    /// <summary>
    /// Set a static or working status.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="icon"></param>
    /// <param name="type"></param>
    public static void Set(string status, string icon = TkIcons.READY, StatusType type = StatusType.Static)
        => Shared.SetInternal(status, icon, type);

    /// <summary>
    /// Set a temporary static or working status.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="icon"></param>
    /// <param name="type"></param>
    /// <param name="duration">The duration of the status in seconds</param>
    public static void SetTemporary(string status, string icon = TkIcons.READY, StatusType type = StatusType.Static, double duration = 2.5)
        => Shared.SetTemporaryInternal(status, icon, type, duration);
    
    /// <inheritdoc cref="Set"/>
    private void SetInternal(string status, string icon = TkIcons.READY, StatusType type = StatusType.Static)
    {
        Status = status;
        Icon = icon;
        Type = type;
    }

    /// <inheritdoc cref="SetTemporary"/>
    private void SetTemporaryInternal(string status, string icon = TkIcons.READY, StatusType type = StatusType.Static, double duration = 2.5)
    {
        Set(status, icon, type);
        _ = Task.Run((Func<Task?>)(async () => {
            await Task.Delay(TimeSpan.FromSeconds(duration));

            if (Status == status) {
                Reset();
            }
        }));
    }

    private void UpdateLoadingStatus(object? _)
    {
        if (Type is StatusType.Static) {
            return;
        }

        Suffix = Suffix.Length switch {
            4 => DOT,
            _ => Suffix + DOT
        };
    }

    partial void OnTypeChanged(StatusType value)
    {
        Suffix = value switch {
            StatusType.Static => string.Empty,
            _ => Suffix
        };
    }
}