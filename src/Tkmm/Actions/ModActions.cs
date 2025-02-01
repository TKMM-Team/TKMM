using System.Diagnostics;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Humanizer;
using Microsoft.Extensions.Logging;
using Tkmm.Core;
using Tkmm.Dialogs;
using TkSharp.Core;
using TkSharp.Core.Models;

namespace Tkmm.Actions;

public sealed partial class ModActions : GuardedActionGroup<ModActions>
{
    protected override string ActionGroupName { get; } = nameof(ModActions).Humanize();

    public Task<TkMod?> Install(object input, Stream? stream = null, TkModContext context = default, CancellationToken ct = default)
        => Install(input, TKMM.ModManager.GetCurrentProfile(), stream, context, ct);

    public async Task<TkMod?> Install(object input, TkProfile profile, Stream? stream = null, TkModContext context = default, CancellationToken ct = default)
    {
        await CanActionRun(showError: false);

        try {
            if (await TKMM.Install(input, stream, context, profile, ct) is not TkMod mod) {
                // TODO: Fetch message template from locale
                TkLog.Instance.LogError("The input of type '{InputType}' ('{Input}') failed to install.",
                    input.GetType(), input);
                return null;
            }

            TkStatus.SetTemporaryShort(Locale[TkLocale.Status_ModSuccessfullyInstalled, mod.Name], TkIcons.CIRCLE_CHECK);
            return mod;
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "An error occured while installing an input of type '{InputType}' ('{Input}').",
                input.GetType(), input);
            await ErrorDialog.ShowAsync(ex);
        }
        finally {
            TkStatus.Reset();
        }

        return null;
    }

    [RelayCommand]
    public async Task ExportMod()
    {
        await CanActionRun(showError: false);

        if (TKMM.ModManager.CurrentProfile?.Selected is not TkProfileMod target) {
            return;
        }

        IStorageFile? result = await App.XamlRoot.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions {
            Title = "Export Mod",
            DefaultExtension = ".tkcl",
            FileTypeChoices = [
                ImportActions.TkclFormat
            ],
            ShowOverwritePrompt = true,
            SuggestedFileName = target.Mod.Name
        });

        if (result is null) {
            return;
        }

        try {
            // TODO: Export packaged mod
            TkStatus.SetTemporaryShort(Locale[TkLocale.Status_ModSuccessfullyExported, target.Mod.Name], TkIcons.CIRCLE_CHECK);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "An error occured while exporting the mod '{ModName}' to '{TargetFile}'.",
                target.Mod.Name, result.Name);
            await ErrorDialog.ShowAsync(ex);
        }
    }

    [RelayCommand]
    public async Task OpenModFolder()
    {
        await CanActionRun(showError: false);

        if (TKMM.ModManager.CurrentProfile?.Selected is not TkProfileMod target) {
            return;
        }

        string outputModFolder = Path.Combine(TKMM.ModManager.ModsFolderPath, target.Mod.Id.ToString());

        try {
            ProcessStartInfo info = new() {
                FileName = outputModFolder,
                UseShellExecute = true
            };

            Process.Start(info);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "An error occured while opening the mod folder for '{ModName}'.",
                target.Mod.Name);
            await ErrorDialog.ShowAsync(ex);
        }
    }

    [RelayCommand]
    public async Task ConfigureModOptions()
    {
        await CanActionRun(showError: false);

        if (TKMM.ModManager.CurrentProfile?.Selected is TkProfileMod target) {
            target.IsEditingOptions = !target.IsEditingOptions;
        }
    }

    public async Task RemoveModFromProfile()
    {
        await CanActionRun(showError: false);

        if (TKMM.ModManager.CurrentProfile?.Selected is not TkProfileMod target) {
            return;
        }

        int removeIndex = TKMM.ModManager.CurrentProfile.Mods.IndexOf(target);
        TKMM.ModManager.CurrentProfile.Mods.RemoveAt(removeIndex);

        if (TKMM.ModManager.CurrentProfile.Mods.Count is 0) {
            return;
        }

        while (removeIndex >= TKMM.ModManager.CurrentProfile.Mods.Count) {
            removeIndex--;
        }

        TKMM.ModManager.CurrentProfile.Selected = TKMM.ModManager.CurrentProfile.Mods[removeIndex];
    }

    [RelayCommand]
    public async Task UninstallMod()
    {
        if (TKMM.ModManager.CurrentProfile?.Selected is not TkProfileMod target) {
            await CanActionRun(showError: false);
            return;
        }

        await UninstallMod(target.Mod);
    }

    public async Task UninstallMod(TkMod target)
    {
        await CanActionRun(showError: false);

        // TODO: Fetch message template from locale
        ContentDialog dialog = new() {
            Title = "Permanently uninstall?",
            Content = $"""
                WARNING: THIS CANNOT BE UNDONE

                Are you sure you would like to permanently uninstall the mod '{target.Name}'?
                """,
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,
            PrimaryButtonText = "Uninstall",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary,
        };

        if (await dialog.ShowAsync() is not ContentDialogResult.Primary) {
            return;
        }

        try {
            TKMM.ModManager.Uninstall(target);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "An error occured while uninstalling the mod '{ModName}'. " +
                                     "Manual cleanup may be required.", target.Name);
            await ErrorDialog.ShowAsync(ex);
        }
    }
}