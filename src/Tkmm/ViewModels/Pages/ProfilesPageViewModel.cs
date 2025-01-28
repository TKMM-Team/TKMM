using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Actions;
using Tkmm.Core;
using TkSharp;
using TkSharp.Core.Models;

namespace Tkmm.ViewModels.Pages;

public partial class ProfilesPageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<TkMod> _filteredMods = GetOrderedMods();

    [ObservableProperty]
    private string? _filterArgument;

    public static TkModManager ModManager => TKMM.ModManager;

    public ProfilesPageViewModel()
    {
        ModManager.Mods.CollectionChanged += (_, _) => {
            FilteredMods = GetOrderedMods();
        };
    }

    [RelayCommand]
    private static void Create()
    {
        TkProfile newProfile = new() {
            Name = Locale[TkLocale.DefaultProfileName, TKMM.ModManager.Profiles.Count + 1]
        };
        
        TKMM.ModManager.Profiles.Add(newProfile);
        TKMM.ModManager.CurrentProfile = newProfile;
    }

    [RelayCommand]
    private static void AddToProfile(TkMod mod)
    {
        TkProfileMod target = mod.GetProfileMod();
        ObservableCollection<TkProfileMod> mods = TKMM.ModManager.GetCurrentProfile().Mods;

        if (mods.Contains(target)) {
            return;
        }

        mods.Add(target);
        TKMM.ModManager.GetCurrentProfile().Selected = target;
    }

    [RelayCommand]
    private static async Task Remove()
    {
        await ModActions.Instance.RemoveModFromProfile();
    }

    [RelayCommand]
    private static void MoveUp()
    {
        TKMM.ModManager.GetCurrentProfile().MoveUp();
    }

    [RelayCommand]
    private static void MoveDown()
    {
        TKMM.ModManager.GetCurrentProfile().MoveDown();
    }

    [RelayCommand]
    private static async Task Uninstall(TkMod? target)
    {
        if (target is null) {
            return;
        }

        await ModActions.Instance.UninstallMod(target);
    }

    [RelayCommand]
    private static Task DeleteCurrentProfile()
    {
        return ProfileActions.Instance.DeleteProfile();
    }

    partial void OnFilterArgumentChanged(string? value)
    {
        if (string.IsNullOrEmpty(value)) {
            FilteredMods = GetOrderedMods();
            return;
        }

        FilteredMods = [..TKMM.ModManager.Mods
            .Where(mod => mod.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase) || value.Contains(mod.Name, StringComparison.InvariantCultureIgnoreCase))
            .OrderBy(mod => mod.Name)
        ];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ObservableCollection<TkMod> GetOrderedMods()
    {
        return [.. TKMM.ModManager.Mods.OrderBy(x => x.Name)];
    }
}
