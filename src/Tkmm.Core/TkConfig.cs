using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;

namespace Tkmm.Core;

public sealed partial class TkConfig : ConfigModule<TkConfig>
{
    public override string Name => "totk";
    
    public TkConfig()
    {
        FileInfo configFileInfo = new(LocalPath);
        if (configFileInfo is { Exists: true, Length: 0 }) {
            File.Delete(LocalPath);
        }
    }
    
    [ObservableProperty]
    private string _gameLanguage = "USen";
    
    [ObservableProperty]
    [property: Config(
        Header = "Keys Folder Path",
        Description = "The absolute path to the folder containing your dumped Switch keys.",
        Group = "Game Dump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        InstanceBrowserKey = "keys-folder-path",
        Title = "Select prod/title keys folder")]
    private string? _keysFolderPath;

    [ObservableProperty]
    [property: Config(
        Header = "Base Game File Path",
        Description = "The absolute path to your dumped base game file (XCI/NSP).",
        Group = "Game Dump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFile,
        Filter = "XCI/NSP:*.nsp;*.xci",
        InstanceBrowserKey = "base-game-file-path",
        Title = "Select base game XCI/NSP")]
    private string? _baseGameFilePath;

    [ObservableProperty]
    [property: Config(
        Header = "Split Base Game Folder Path",
        Description = "The path to a folder containing your game split into multiple files (only useful for FAT32 file systems).",
        Group = "Game Dump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        InstanceBrowserKey = "split-files-path",
        Title = "Select split files folder")]
    private string? _splitFilesPath;

    // TODO: Support version dropdown
    [ObservableProperty]
    [property: Config(
        Header = "Game Update File Path",
        Description = "The absolute path to your dumped game update file (NSP).",
        Group = "Game Dump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFile,
        Filter = "NSP:*.nsp",
        InstanceBrowserKey = "game-update-file-path",
        Title = "Select game update NSP")]
    private string? _gameUpdateFilePath;

    [ObservableProperty]
    [property: Config(
        Header = "SD Card Root Path",
        Description = "The path to the root of your SD card or emuMMC (must contain a Nintendo/Contents folder with the base game or update installed).",
        Group = "Game Dump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        InstanceBrowserKey = "sd-card-root-path",
        Title = "Select SD card root folder")]
    private string? _sdCardRootPath;

    // TODO: Support multiple game paths
    [ObservableProperty]
    [property: Config(
        Header = "Game Dump Folder Path",
        Description = "The absolute path to your dumped RomFS folder.",
        Group = "Game Dump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        InstanceBrowserKey = "game-dump-folder-path",
        Title = "Select game dump folder path (with update 1.1.0 or later)")]
    [property: JsonPropertyName("GamePath")]
    private string? _gameDumpFolderPath;
}