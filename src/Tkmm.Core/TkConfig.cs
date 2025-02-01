using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;
using Tkmm.Core.Models;
using TkSharp.Data.Embedded;
using TkSharp.Extensions.LibHac;

namespace Tkmm.Core;

public sealed partial class TkConfig : ConfigModule<TkConfig>
{
    public const string DEFAULT_GAME_VERSION = "Any";
    
    [JsonIgnore]
    public override string Name => "totk";
    
    public TkConfig()
    {
        FileInfo configFileInfo = new(LocalPath);
        if (configFileInfo is { Exists: true, Length: 0 }) {
            File.Delete(LocalPath);
        }
    }

    [ObservableProperty]
    [property: Config(
        Header = "Preferred Game Version",
        Description = "The game version to look for when reading the configured SD card if multiple versions are found.",
        Group = "Game Dump")]
    [property: DropdownConfig(DEFAULT_GAME_VERSION, "1.2.1", "1.2.0", "1.1.2", "1.1.1", "1.1.0")]
    private string _preferredGameVersion = DEFAULT_GAME_VERSION;
    
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
        Header = "Packaged Base Game Path",
        Description = "The absolute path to your dumped base game file (.xci or .nsp) or split folder.",
        Group = "Game Dump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFile,
        Filter = "XCI/NSP:*.nsp;*.xci|All files:*.*",
        InstanceBrowserKey = "base-game-file-path",
        Title = "Select base game XCI/NSP")]
    private FileOrFolder _packagedBaseGamePath;

    // TODO: Support version dropdown
    [ObservableProperty]
    [property: Config(
        Header = "Game Update File Path",
        Description = "The absolute path to your dumped game update file (.xci or .nsp) or split folder.",
        Group = "Game Dump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFile,
        Filter = "NSP:*.nsp|All files:*.*",
        InstanceBrowserKey = "game-update-file-path",
        Title = "Select game update NSP")]
    private FileOrFolder _packagedUpdatePath;

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

    public TkExtensibleRomProvider CreateRomProvider()
    {
        using Stream checksums = TkEmbeddedDataSource.GetChecksumsBin();
        
        return TkExtensibleRomProviderBuilder.Create(checksums)
            .WithKeysFolder(() => KeysFolderPath)
            .WithExtractedGameDump(() => GameDumpFolderPath)
            .WithSdCard(() => SdCardRootPath)
            .WithPackagedBaseGame(() => PackagedBaseGamePath)
            .WithPackagedUpdate(() => PackagedUpdatePath)
            .Build();
    }
}