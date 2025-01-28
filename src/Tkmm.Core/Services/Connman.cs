using System.Buffers;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Tkmm.Core.Helpers;
using Tkmm.Core.Models.NX;
using TkSharp.Core;

namespace Tkmm.Core.Services;

public static class Connman
{
    private static ReadOnlySpan<byte> HexAlphabetBytes => "0123456789ABCDEF"u8;
    
    private static readonly SearchValues<char> _propertyBreakSearchValues = SearchValues.Create("],");
    
#if TARGET_NX
    public const string CONNMAN_DIR = "/storage/.cache/connman/";
#else
    public const string CONNMAN_DIR = "/var/lib/connman/";
#endif
    
    private const string RESTART_COMMAND = "systemctl restart connman";
    private const string SCAN_WIFI_COMMAND = "connmanctl scan wifi";
    private const string GET_SERVICES_COMMAND = "connmanctl services";
    private const string GET_SERVICE_COMMAND = "connmanctl services {0}";
    private const string CONNECT_COMMAND = "connmanctl connect {0}";
    private const string DISCONNECT_COMMAND = "connmanctl disconnect {0}";
    private const string FORGET_COMMAND = "connmanctl config {0} --remove";
    private const string GET_TECHNOLOGIES_COMMAND = "connmanctl technologies";
    private const string ENABLE_TECHNOLOGY_COMMAND = "connmanctl enable {0}";
    private const string DISABLE_TECHNOLOGY_COMMAND = "connmanctl disable {0}";

    public static async ValueTask Scan(CancellationToken ct = default)
    {
        await NxProcessHelper.ExecAsync(SCAN_WIFI_COMMAND, ct);
    }
    
    public static async IAsyncEnumerable<NxNetwork> GetNetworks([EnumeratorCancellation] CancellationToken ct = default)
    {
        using StreamReader services = NxProcessHelper.ReadCommand(GET_SERVICES_COMMAND);

        while (await services.ReadLineAsync(ct) is string service) {
            ReadOnlySpan<char> line = service.AsSpan();

            if (line.Length < 10) {
                continue;
            }

            int lastSpaceIndex = line.LastIndexOf(' ') + 1;
            if (line[lastSpaceIndex..(lastSpaceIndex + 5)] is not "wifi_") {
                continue;
            }

            string id = service[lastSpaceIndex..];

            if (id.Contains("hidden_managed")) {
                continue;
            }
            
            string ssid = service[3..(lastSpaceIndex - 1)].Trim();
            
            NxNetwork network = new(id, ssid) {
                IsConnected = line[2] is 'R' or 'O', // Roaming or Offline
                IsKnown = line[0] is '*',
            };
            
            yield return network;
        }
    }

    public static async ValueTask LoadNetworkProperties(NxNetwork network)
    {
        using StreamReader properties = NxProcessHelper.ReadCommand(string.Format(GET_SERVICE_COMMAND, network.Id));

        while (await properties.ReadLineAsync() is string property) {
            ReadOnlySpan<char> line = property.AsSpan();

            if (line.Length < 2 || line[0] is not ' ' || line[1] is not ' ') {
                continue;
            }

            int keyEndIndex = line.IndexOf('=') - 1;

            switch (property[2..keyEndIndex]) {
                case "IPv4":
                    network.IpAddress = FindPropertyValue(property, line, "Address");
                    network.SubnetMask = FindPropertyValue(property, line, "Netmask");
                    network.Gateway = FindPropertyValue(property, line, "Gateway");
                    continue;
            };
        }
    }
    
    
    public static async ValueTask Connect(NxNetwork network, CancellationToken ct = default)
    {
        string settingsFolderPath = Path.Combine(CONNMAN_DIR, network.Id);
        string settingsFilePath = Path.Combine(settingsFolderPath, "settings");

        if (network.IsKnown) {
            goto Connect;
        }

        Directory.CreateDirectory(settingsFolderPath);
        
        await using (StreamWriter writer = File.CreateText(settingsFilePath)) {
            await writer.WriteLineAsync($"[{network.Id}]");
            await writer.WriteLineAsync($"Name={network.Ssid}");
            await writer.WriteLineAsync($"SSID={Convert.ToHexStringLower(Encoding.UTF8.GetBytes(network.Ssid))}");
            await writer.WriteLineAsync("Favorite=true");
            await writer.WriteLineAsync("AutoConnect=true");
            await writer.WriteLineAsync($"Passphrase={network.Passphrase}");
            await writer.WriteLineAsync("IPv4.method=dhcp");
        }
        
        await Restart(ct);
        
    Connect:
        await NxProcessHelper.ExecAsync(string.Format(CONNECT_COMMAND, network.Id), ct);
    }

    public static async ValueTask Disconnect(NxNetwork network, CancellationToken ct = default)
    {
        await NxProcessHelper.ExecAsync(string.Format(DISCONNECT_COMMAND, network.Id), ct);
    }

    public static async ValueTask Forget(NxNetwork network, CancellationToken ct = default)
    {
        await NxProcessHelper.ExecAsync(string.Format(FORGET_COMMAND, network.Id), ct);
    }

    public static void UpdateTechnology(string name, bool enable)
    {
        NxProcessHelper.Exec(string.Format(enable switch {
            true => ENABLE_TECHNOLOGY_COMMAND,
            false => DISABLE_TECHNOLOGY_COMMAND,
        }, name));
    }

    public static bool IsTechnologyEnabled(string name)
    {
        bool isFound = false;
        
        using StreamReader technologies = NxProcessHelper.ReadCommand(GET_TECHNOLOGIES_COMMAND);
        while (technologies.ReadLine() is string technology) {
            ReadOnlySpan<char> line = technology.AsSpan();

            switch (isFound) {
                case true:
                    goto ParseArgs;
                case false when line[0] is not ' ':
                    isFound = line.LastIndexOf('/') is var lastSlashIndex and > -1 && line[(lastSlashIndex + 1)..].SequenceEqual(name);
                    continue;
            }
            
            continue;

        ParseArgs:
            if (line.Length < 2 || line[0] is not ' ' || line[1] is not ' ') {
                continue;
            }

            if (line.Length > 10 && line[2..9] is "Powered") {
                return line[12..] is "True";
            }
        }
        
        return false;
    }
    
    public static string? GetMacAddress()
    {
        PhysicalAddress? address = NetworkInterface.GetAllNetworkInterfaces()
            .FirstOrDefault(x => x is { OperationalStatus: OperationalStatus.Up, NetworkInterfaceType: NetworkInterfaceType.Ethernet })?
            .GetPhysicalAddress();

        if (address is null) {
            return null;
        }

        Span<byte> addressBytes = address.GetAddressBytes();
        int formattedStringLength = addressBytes.Length * 2 + addressBytes.Length - 1;

        return string.Create(formattedStringLength, addressBytes, static (result, address) => {
            int offset = -1;
            for (int i = 0; i < address.Length;) {
                result[++offset] = (char)HexAlphabetBytes[address[i] >> 4];
                result[++offset] = (char)HexAlphabetBytes[address[i] & 0xF];

                if (address.Length > ++i) {
                    result[++offset] = ':';
                }
            }
        });
    }

    private static string FindPropertyValue(string property, ReadOnlySpan<char> line, ReadOnlySpan<char> key)
    {
        try {
            int index = line.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            int breakIndex = index + line[index..].IndexOfAny(_propertyBreakSearchValues);

            while (line[breakIndex] is ' ') {
                breakIndex--;
            }

            return property[(index + key.Length + 1)..breakIndex];
        }
        catch (Exception) {
            TkLog.Instance.LogWarning(
                "The property '{PropertyName}' could not be found in {PropertyLine}",
                key.ToString(), property);
            return string.Empty;
        }
    }

    private static async ValueTask Restart(CancellationToken ct = default)
    {
        await NxProcessHelper.ExecAsync(RESTART_COMMAND, ct);
    }
}