﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Avalonia.Controls;
using ReactiveUI;
using Tkmm.Managers;

namespace Tkmm.ViewModels.Pages
{
    public class NetworkSettingsPageViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Connman.WifiNetworkInfo> availableNetworks;
        private Connman.WifiNetworkInfo? selectedNetwork;
        private string networkPassword;
        private readonly Connman.ConnmanT connman;

        public NetworkSettingsPageViewModel()
        {
            connman = Connman.ConnmanctlInit();
            AvailableNetworks = new ObservableCollection<Connman.WifiNetworkInfo>();
            ConnectToNetworkCommand = ReactiveCommand.Create(ConnectToNetwork);
            ScanForNetworksCommand = ReactiveCommand.Create(ScanForNetworks);
            ForgetSsidCommand = ReactiveCommand.Create(ForgetSsid);
            ScanForNetworks();
        }

        public ObservableCollection<Connman.WifiNetworkInfo> AvailableNetworks
        {
            get => availableNetworks;
            set
            {
                availableNetworks = value;
                OnPropertyChanged(nameof(AvailableNetworks));
            }
        }

        public Connman.WifiNetworkInfo? SelectedNetwork
        {
            get => selectedNetwork;
            set
            {
                selectedNetwork = value;
                OnPropertyChanged(nameof(SelectedNetwork));
            }
        }

        public string NetworkPassword
        {
            get => networkPassword;
            set
            {
                networkPassword = value;
                OnPropertyChanged(nameof(NetworkPassword));
            }
        }

        public ICommand ForgetSsidCommand { get; }
        public ICommand ConnectToNetworkCommand { get; }
        public ICommand ScanForNetworksCommand { get; }

        private void ForgetSsid()
        {
            if (SelectedNetwork.HasValue)
            {
                var network = SelectedNetwork.Value;
                bool result = Connman.ConnmanctlForgetSsid(connman, network);
                if (result)
                {
                    AvailableNetworks.Remove(network);
                    SelectedNetwork = null;
                }
            }
        }

        private async void ConnectToNetwork()
        {
            if (SelectedNetwork.HasValue)
            {
                var network = SelectedNetwork.Value;
                network.Passphrase = NetworkPassword;
                Connman.ConnmanctlConnectSsid(connman, network);
                await Task.Delay(3500);
                ScanForNetworks();
            }
        }

        private async void ScanForNetworks()
        {
            Connman.ConnmanctlScan(connman);
            await Task.Delay(3500);
            UpdateAvailableNetworks();
        }

        private void UpdateAvailableNetworks()
        {
            var networks = Connman.ConnmanctlGetSsids(connman).NetList;

            if (networks == null)
            {
                AvailableNetworks = new ObservableCollection<Connman.WifiNetworkInfo>();
            }
            else
            {
                AvailableNetworks = new ObservableCollection<Connman.WifiNetworkInfo>(
                    networks.Where(n => !string.IsNullOrEmpty(n.Ssid)));
            }
        }

        private class WifiNetworkInfoComparer : IEqualityComparer<Connman.WifiNetworkInfo>
        {
            public bool Equals(Connman.WifiNetworkInfo x, Connman.WifiNetworkInfo y)
            {
                return x.Ssid == y.Ssid && x.NetId == y.NetId;
            }

            public int GetHashCode(Connman.WifiNetworkInfo obj)
            {
                return HashCode.Combine(obj.Ssid, obj.NetId);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}