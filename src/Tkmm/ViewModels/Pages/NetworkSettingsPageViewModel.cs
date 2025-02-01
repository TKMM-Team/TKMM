using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Core.Models.NX;
using Tkmm.Core.Services;

namespace Tkmm.ViewModels.Pages;

public partial class NetworkSettingsPageViewModel : ObservableObject
{
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Timer _autoRefreshTimer; 
    
    public List<NxNetworkService> NetworkServices { get; }

    public NxNetworkService WiFiService { get; }

    [ObservableProperty]
    private ObservableCollection<NxNetwork> _networks = [];

    [ObservableProperty]
    private NxNetwork? _connected;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _macAddress;

    public NetworkSettingsPageViewModel()
    {
        WiFiService = new NxNetworkService(Locale[TkLocale.NetworkSettings_WiFiService_Name], OnWiFiEnabledChanged) {
            IsEnabled = Connman.IsTechnologyEnabled("wifi")
        };

        NetworkServices = [
            WiFiService,
            new NxNetworkService(Locale[TkLocale.NetworkSettings_SshService_Name], OnSshEnabledChanged) {
                IsEnabled = NxServices.IsSshEnabled()
            },
            new NxNetworkService(Locale[TkLocale.NetworkSettings_SmbService_Name], OnSmbEnabledChanged) {
                IsEnabled = NxServices.IsSmbEnabled()
            }
        ];

        _macAddress = Connman.GetMacAddress();
        _autoRefreshTimer = new Timer(state => _ = Task.Run(() => RefreshNetworksInternal()));
        _autoRefreshTimer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1));
    }

    [RelayCommand]
    private async Task LocateNetworks()
    {
        IsLoading = true;
        Networks.Clear();
        
        await Task.Run(async () => {
            await Connman.Scan();
            await RefreshNetworksInternal(force: true);
        });
        
        IsLoading = false;
    }

    private void OnWiFiEnabledChanged(bool isEnabled)
    {
        Connman.UpdateTechnology("wifi", isEnabled);

        if (isEnabled) {
            _ = Task.Run(async () => await LocateNetworks());
        }
        else {
            Networks.Clear();
            Connected = null;
        }
    }

    private static void OnSshEnabledChanged(bool isEnabled)
    {
        if (isEnabled) {
            NxServices.EnableSsh();
            return;
        }

        NxServices.DisableSsh();
    }

    private static void OnSmbEnabledChanged(bool isEnabled)
    {
        if (isEnabled) {
            NxServices.EnableSmb();
            return;
        }

        NxServices.DisableSmb();
    }

    private async Task RefreshNetworksInternal(bool force = false, CancellationToken ct = default)
    {
        if (IsLoading && !force) {
            return;
        }
        
        HashSet<string> foundNetworks = [];
        
        await foreach (NxNetwork network in Connman.GetNetworks(ct)) {
            if (Networks.FirstOrDefault(net => net.Id == network.Id) is NxNetwork existing) {
                if (existing.IsConnected) {
                    await SetConnected(network);
                }
                
                existing.IsConnected = network.IsConnected;
                existing.IsKnown = network.IsKnown;
                goto UpdateTrackingList;
            }

            if (network.IsConnected) {
                await SetConnected(network);
            }
            
            Networks.Add(network);
            
        UpdateTrackingList:
            foundNetworks.Add(network.Id);
        }

        for (int i = 0; i < Networks.Count; i++) {
            if (foundNetworks.Contains(Networks[i].Id)) {
                continue;
            }
            
            Networks.RemoveAt(i);
            i--;
        }
    }

    private async Task SetConnected(NxNetwork network)
    {
        await Task.Run(async () => await Connman.LoadNetworkProperties(network));

        if (Connected is not null && network.Id == Connected.Id) {
            Connected.UpdateProperties(network);
            return;
        }
        
        Connected = network;
    }
}