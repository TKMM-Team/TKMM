using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;
using Tkmm.Core.Models;
using TkSharp.Extensions.GameBanana;

namespace Tkmm.Core;

public sealed partial class Config : ConfigModule<Config>
{   
    public override string Name => "tkmm";
    
    public event Action<string> ThemeChanged = delegate { };
    
    [JsonIgnore]
    public Func<List<string>>? GetLanguages { get; set; }

    public string[] GameLanguages { get; set; } = [
        "USen", "EUen", "JPja", "EUfr", "USfr", "USes", "EUes", "EUde", "EUnl", "EUit", "EUru", "KRko", "CNzh", "TWzh"
    ];
    
    public static void SaveAll()
    {
        TkConfig.Shared.Save();
        GbConfig.Shared.Save();
        Shared.Save();
    }

    public Config()
    {
        FileInfo configFileInfo = new(LocalPath);
        if (configFileInfo is { Exists: true, Length: 0 }) {
            File.Delete(LocalPath);
        }
    }
    
    [ObservableProperty]
    [property: Config(
        Header = "Theme",
        Description = "",
        Group = "Application")]
    [property: DropdownConfig("Dark", "Light")]
    private string _theme = "Dark";

    partial void OnThemeChanged(string value)
    {
        ThemeChanged(value);
    }
    
    [ObservableProperty]
    [property: Config(
        Header = "System Language",
        Description = "The language to use in the user interface (restart required)",
        Group = "Application")]
    [property: DropdownConfig(RuntimeItemsSourceMethodName = nameof(GetLanguagesInternal))]
    private string _cultureName = "en_US";

    [ObservableProperty]
    [property: Config(
        Header = "Auto Save Settings",
        Description = "Automatically save the settings when a change is made and there are no errors.",
        Group = "Application")]
    private bool _autoSaveSettings = true;
    
#if SWITCH
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public string SevenZipPath => "/usr/bin/7zz";
#else
    [ObservableProperty]
    [property: Config(
        Header = "7z Path",
        Description = "The absolute path to the 7-zip executable used for faster 7z extraction.",
        Group = "Application")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFile,
        InstanceBrowserKey = "7z-path",
        Filter = "7z:*7z*",
        Title = "7z Location")]
    private string? _sevenZipPath;
#endif
    
#if !SWITCH
    [property: Config(
        Header = "Emulator Executable Path",
        Description = "The absolute path to your emulator's executable.",
        Group = "Application")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFile,
        InstanceBrowserKey = "emulator-path",
#if TARGET_WINDOWS
        Filter = "Executable:*.exe|All files:*",
#else
        Filter = "Executable:*",
#endif
        Title = "Select emulator executable")]
    [ObservableProperty]
    private string? _emulatorPath;
#endif
    
    [ObservableProperty]
    [property: Config(
        Header = "Show Trivia Popup",
        Description = "Show the trivia popup when merging.",
        Group = "Application")]
    private bool _showTriviaPopup = true;
    
    [ObservableProperty]
    [property: Config(
        Header = "Default Author",
        Description = "The default author used when packaging TKCL mods.",
        Group = "Packaging")]
    private string _defaultAuthor = string.Empty;
    
    [ObservableProperty]
    [property: Config(
        Header = "Target Language",
        Description = "The target language that MalsMerger should create an archive for.",
        Group = "Merging")]
    [property: DropdownConfig("USen", "EUen", "JPja", "EUfr", "USfr", "USes", "EUes", "EUde", "EUnl", "EUit", "EUru", "KRko", "CNzh", "TWzh")]
    private string _gameLanguage = "USen";
    
#if !SWITCH
    [ObservableProperty]
    [property: Config(
        Header = "Export Locations",
        Description = "Define custom locations to export the merged mod to.",
        Group = "Merging")]
    private ExportLocations _exportLocations = [];

    [ObservableProperty]
    [property: Config(
        Header = "Merge Output Folder",
        Description = "The location to write the merged output to. (Default location is './Merged' next to the TKMM executable)",
        Group = "Merging")]
    private string? _mergeOutput;
#endif
    
    [ObservableProperty]
    private GameBananaSortMode _gameBananaSortMode = GameBananaSortMode.Default;

    public bool ConfigExists()
    {
        return File.Exists(LocalPath);
    }

    public List<string> GetLanguagesInternal() => GetLanguages?.Invoke() ?? ["en_US"];
}