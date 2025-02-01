using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
using Tkmm.Core;
using Tkmm.Core.Helpers;
using Tkmm.Dialogs;
using TkSharp;
using TkSharp.Core;
using TkSharp.Core.Models;
using TkSharp.Packaging;

namespace Tkmm.ViewModels.Pages;

public sealed partial class ProjectsPageViewModel : ObservableObject
{
    private readonly List<string> _deletions = [];
    
    public ProjectsPageViewModel()
    {
        TkProjectManager.Load();
    }
    
    [ObservableProperty]
    private TkProject? _project;
    
    [RelayCommand]
    private async Task NewProject()
    {
        if (await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Create a TotK mod project folder." }) is not [IStorageFolder folder]) {
            TkLog.Instance.LogInformation("Folder picker operation returned an invalid result or was cancelled.");
            return;
        }

        if (folder.TryGetLocalPath() is not string localFolderPath) {
            TkLog.Instance.LogError(
                "Storage folder {Folder} could not be converted into a local folder path.",
                folder);
            return;
        }
        
        Project = TkProjectManager.NewProject(localFolderPath);
        TkProjectManager.Save();
    }
    
    [RelayCommand]
    private async Task OpenProject()
    {
        FilePickerOpenOptions filePickerOpenOptions = new() {
            Title = "Open a TotK mod project.",
            AllowMultiple = false,
            FileTypeFilter = [
                new FilePickerFileType("TotK Project") {
                    Patterns = [
                        "*.tkproj"
                    ]
                }
            ]
        };
        
        if (await App.XamlRoot.StorageProvider.OpenFilePickerAsync(filePickerOpenOptions) is not [IStorageFile file]) {
            TkLog.Instance.LogInformation("File picker operation returned an invalid result or was cancelled.");
            return;
        }

        if (file.TryGetLocalPath() is not string localFilePath || Path.GetDirectoryName(localFilePath) is not string localFolderPath) {
            TkLog.Instance.LogError(
                "Storage file {File} could not be converted into a local file path.",
                file);
            return;
        }
        
        Project = TkProjectManager.OpenProject(localFolderPath);
        TkProjectManager.Save();
    }
    
    [RelayCommand]
    private void Exit()
    {
        Project = null;
    }
    
    [RelayCommand]
    private void Save()
    {
        if (Project is null) {
            return;
        }
        
        TkStatus.Set($"Saving '{Project.Mod.Name}'", "fa-regular fa-floppy-disk-circle-arrow-right", StatusType.Working);
        
        ApplyDeletions();
        Project.Save();
        TkProjectManager.Save();
        
        TkStatus.SetTemporary($"Saved '{Project.Mod.Name}'", "fa-regular fa-circle-check");
    }
    
    [RelayCommand]
    private async Task Package()
    {
        if (Project is null) {
            return;
        }
        
        FilePickerSaveOptions filePickerOptions = new() {
            Title = "Export TotK changelog package.",
            SuggestedFileName = $"{Project.Mod.Name}.tkcl",
            DefaultExtension = ".tkcl",
            FileTypeChoices = [
                new FilePickerFileType("TotK Changelog Package") {
                    Patterns = [
                        "*.tkcl"
                    ]
                }
            ]
        };
        
        if (await App.XamlRoot.StorageProvider.SaveFilePickerAsync(filePickerOptions) is not IStorageFile file) {
            TkLog.Instance.LogInformation("File picker operation returned an invalid result or was cancelled.");
            return;
        }

        TkStatus.Set($"Packaging '{Project.Mod.Name}'", "fa-regular fa-boxes-packing", StatusType.Working);
        
        await using Stream output = await file.OpenWriteAsync();

        using ITkRom tkRom = TKMM.GetTkRom();
        await Project.Package(output, tkRom);
        
        TkStatus.SetTemporary($"Packaged '{Project.Mod.Name}'", "fa-regular fa-box-circle-check");
    }
    
    [RelayCommand]
    private async Task Install()
    {
        if (Project is null) {
            return;
        }
        
        TkStatus.Set($"Installing '{Project.Mod.Name}'", "fa-regular fa-download", StatusType.Working);

        ITkModWriter writer = TKMM.ModManager.GetSystemWriter(new TkModContext(Project.Mod.Id));
        using ITkRom tkRom = TKMM.GetTkRom();
        await Project.Build(writer, tkRom, TKMM.ModManager.GetSystemSource(Project.Mod.Id.ToString()));
        
        TkStatus.SetTemporary($"Installed '{Project.Mod.Name}'", "fa-regular fa-circle-check");
    }

    [RelayCommand]
    private async Task EditContributors(ContentControl contributionsEditor)
    {
        TaskDialog dialog = new() {
            XamlRoot = App.XamlRoot,
            Title = "Edit Contributors",
            DataContext = this,
            Content = contributionsEditor,
            Buttons = [
                TaskDialogButton.CloseButton
            ]
        };

        await dialog.ShowAsync();
        dialog.Content = null;
    }

    [RelayCommand]
    private static async Task BrowseThumbnail(TkItem parent)
    {
        FilePickerOpenOptions filePickerOpenOptions = new() {
            Title = "Open an image for the mod thumbnail.",
            AllowMultiple = false,
            FileTypeFilter = [
                FilePickerFileTypes.ImageAll
            ]
        };
        
        if (await App.XamlRoot.StorageProvider.OpenFilePickerAsync(filePickerOpenOptions) is not [IStorageFile file]) {
            TkLog.Instance.LogInformation("File picker operation returned an invalid result or was cancelled.");
            return;
        }
        
        if (file.TryGetLocalPath() is not string localFilePath) {
            TkLog.Instance.LogError(
                "Storage file {File} could not be converted into a local file path.", file);
            return;
        }

        parent.Thumbnail = new TkThumbnail {
            ThumbnailPath = localFilePath,
            Bitmap = new Bitmap(localFilePath)
        };
    }

    [RelayCommand]
    private async Task Refresh()
    {
        if (Project is null) {
            return;
        }
        
        if (await MessageDialog.Show("Unsaved changes will be lost, would you like to proceed?", "Warning!", MessageDialogButtons.YesNoCancel) is not ContentDialogResult.Primary) {
            return;
        }
        
        Project.Refresh();
    }

    [RelayCommand]
    private void RemoveOptionGroup(TkModOptionGroup group)
    {
        if (Project is null || !Project.TryGetPath(group, out string? groupFolderPath)) {
            return;
        }

        if (Project.Mod.OptionGroups.Remove(group) && Directory.Exists(groupFolderPath)) {
            _deletions.Add(groupFolderPath);
        }
    }
    
    [RelayCommand]
    private async Task ImportOptionGroup()
    {
        if (Project is null) {
            return;
        }
        
        if (await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Create a TotK mod project folder." }) is not [IStorageFolder folder]) {
            TkLog.Instance.LogInformation("Folder picker operation returned an invalid result or was cancelled.");
            return;
        }

        if (folder.TryGetLocalPath() is not string localFolderPath) {
            TkLog.Instance.LogError(
                "Storage folder {Folder} could not be converted into a local folder path.",
                folder);
            return;
        }
        
        string name = Path.GetFileName(localFolderPath);
        string output = Path.Combine(Project.FolderPath, "options", name);
        
        if (Directory.Exists(output)) {
            ContentDialog warningDialog = new() {
                Title = "Warning",
                Content = $"The option group '{name}' already exists, would you like to replace it?",
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
            };

            if (await warningDialog.ShowAsync() is not ContentDialogResult.Primary) {
                return;
            }

            Directory.Delete(output, recursive: true);

            if (Project.Mod.OptionGroups.FirstOrDefault(x => Project.TryGetPath(x, out string? optionGroupFolderPath) && optionGroupFolderPath == output) is TkModOptionGroup target) {
                Project.Mod.OptionGroups.Remove(target);
            }
        }

        DirectoryHelper.Copy(localFolderPath, output, overwrite: true);
        TkProjectManager.LoadOptionGroupFolder(Project, output);
    }

    [RelayCommand]
    private void RemoveOption(TkModOption option)
    {
        if (Project?.Mod.OptionGroups.FirstOrDefault(x => x.Options.Contains(option)) is not TkModOptionGroup group
                || !Project.TryGetPath(option, out string? optionFolderPath)) {
            return;
        }

        if (group.Options.Remove(option) && Directory.Exists(optionFolderPath)) {
            _deletions.Add(optionFolderPath);
        }
    }

    [RelayCommand]
    private async Task ImportOption(TkModOptionGroup group)
    {
        if (Project is null || !Project.TryGetPath(group, out string? groupFolderPath)) {
            return;
        }
        
        if (await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Create a TotK mod project folder." }) is not [IStorageFolder folder]) {
            TkLog.Instance.LogInformation("Folder picker operation returned an invalid result or was cancelled.");
            return;
        }

        if (folder.TryGetLocalPath() is not string localFolderPath) {
            TkLog.Instance.LogError(
                "Storage folder {Folder} could not be converted into a local folder path.",
                folder);
            return;
        }
        
        string name = Path.GetFileName(localFolderPath);
        string output = Path.Combine(groupFolderPath, name);
        
        if (Directory.Exists(output)) {
            ContentDialog warningDialog = new() {
                Title = "Warning",
                Content = $"The option group '{name}' already exists, would you like to replace it?",
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
            };

            if (await warningDialog.ShowAsync() is not ContentDialogResult.Primary) {
                return;
            }

            Directory.Delete(output, recursive: true);

            if (group.Options.FirstOrDefault(x => Project.TryGetPath(x, out string? optionFolderPath) && optionFolderPath == output) is TkModOption target) {
                group.Options.Remove(target);
            }
        }

        DirectoryHelper.Copy(localFolderPath, output, overwrite: true);
        TkProjectManager.LoadOptionFolder(Project, group, output);
    }

    private void ApplyDeletions()
    {
        foreach (string path in _deletions.Where(Directory.Exists)) {
            Directory.Delete(path, true);
        }
        
        _deletions.Clear();
    }
}