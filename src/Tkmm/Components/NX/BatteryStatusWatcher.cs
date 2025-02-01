#if SWITCH

using CommunityToolkit.HighPerformance;
using Tkmm.ViewModels;

namespace Tkmm.Components.NX;

public static class BatteryStatusWatcher
{
    private static readonly Timer _autoUpdateBatteryTimer = new(state => {
        _ = Task.Run(() => CheckTarget(STATUS_FILE_PATH, CheckStatus));
        _ = Task.Run(() => CheckTarget(CHARGE_FILE_PATH, CheckCharge));
    });
    
    private const string CHARGING = "fa-solid fa-battery-bolt";
    private const string CHARGE_EMPTY = "fa-solid fa-battery-empty";
    private const string CHARGE_LOW = "fa-solid fa-battery-low";
    private const string CHARGE_QUARTER = "fa-solid fa-battery-quarter";
    private const string CHARGE_HALF = "fa-solid fa-battery-half";
    private const string CHARGE_THREE_QUARTERS = "fa-solid fa-battery-three-quarters";
    private const string CHARGE_FULL = "fa-solid fa-battery-full";
    private const string CHARGE_ERROR = "fa-regular fa-battery-exclamation";
    
#if TARGET_NX
    private const string STATUS_FILE_PATH = "/sys/class/power_supply/battery/status";
    private const string CHARGE_FILE_PATH = "/sys/class/power_supply/battery/capacity";
#else
    private const string STATUS_FILE_PATH = "/home/archleaders/tkmm-nx/battery/status";
    private const string CHARGE_FILE_PATH = "/home/archleaders/tkmm-nx/battery/capacity";
#endif
    
    public static void Start()
    {
        ShellViewModel.Shared.BatteryIcon = GetChargeIcon(out int charge);
        ShellViewModel.Shared.BatteryCharge = charge;
        
        _autoUpdateBatteryTimer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1));
    }

    private static void CheckTarget(string path, Action<Stream> process)
    {
        using FileStream fs = File.OpenRead(path);
        process(fs);
    }

    private static void CheckStatus(Stream input)
    {
        Span<char> status = stackalloc char[4];
        int read = input.Read(status.Cast<char, byte>()) / 2;

        string chargeIcon = GetChargeIcon(out int charge);
        
        string batteryIcon = status[..read] switch {
            // Unicode representation of "Charging" in UTF8
            "桃牡楧杮" => CHARGING,
            _ => chargeIcon
        };
        
        ShellViewModel.Shared.BatteryIcon = batteryIcon;
        ShellViewModel.Shared.BatteryCharge = charge;
    }

    private static void CheckCharge(Stream input)
    {
        if (ShellViewModel.Shared.BatteryIcon == CHARGING) {
            return;
        }
        
        ShellViewModel.Shared.BatteryIcon = GetChargeIcon(input, out int charge);
        ShellViewModel.Shared.BatteryCharge = charge;
    }

    private static string GetChargeIcon(out int charge)
    {
        using FileStream fs = File.OpenRead(CHARGE_FILE_PATH);
        return GetChargeIcon(fs, out charge);
    }

    private static unsafe string GetChargeIcon(Stream input, out int charge)
    {
        Span<byte> chargeUtf8 = stackalloc byte[4];
        int read = input.Read(chargeUtf8);

        if (!int.TryParse(chargeUtf8[..read], out charge)) {
            charge = -1;
        }

        return charge switch {
            -1 => CHARGE_ERROR,
            < 8 => CHARGE_EMPTY,
            < 21 => CHARGE_LOW,
            < 41 => CHARGE_QUARTER,
            < 66 => CHARGE_HALF,
            < 88 => CHARGE_THREE_QUARTERS,
            < 101 => CHARGE_FULL,
            _ => CHARGE_ERROR,
        };
    }
}
#endif