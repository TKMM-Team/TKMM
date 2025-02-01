using System.Reflection;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;
using Tkmm.Core.Models;

namespace Tkmm.Builders;

public partial class FileOrFolderControlContext(IConfigModule context, PropertyInfo propertyInfo) : ObservableObject
{
    private readonly IConfigModule _context = context;
    private readonly PropertyInfo _propertyInfo = propertyInfo;

    [ObservableProperty]
    private string? _target = ((FileOrFolder)context.Properties[propertyInfo.Name].Property.GetValue(context)!).Path;

    [RelayCommand]
    private async Task OpenFile()
    {
        FilePickerOpenOptions options = new() {
            Title = Locale[TkLocale.Tip_BrowseFile],
            AllowMultiple = false
        };
        
        if (_propertyInfo.GetCustomAttribute<BrowserConfigAttribute>() is BrowserConfigAttribute browserConfig) {
            options.Title = browserConfig.Title;
            options.SuggestedFileName = browserConfig.SuggestedFileName;
            options.FileTypeFilter = ParseFilterString(browserConfig.Filter);
        }
        
        IReadOnlyList<IStorageFile> result = await App.XamlRoot.StorageProvider.OpenFilePickerAsync(options);
        
        if (result is not [IStorageFile target] || target.TryGetLocalPath() is not string path) {
            return;
        }

        Target = path;
    }

    [RelayCommand]
    private async Task OpenFolder()
    {
        FolderPickerOpenOptions options = new() {
            Title = Locale[TkLocale.Tip_BrowseFolder],
            AllowMultiple = false
        };
        
        if (_propertyInfo.GetCustomAttribute<BrowserConfigAttribute>() is BrowserConfigAttribute browserConfig) {
            options.Title = browserConfig.Title;
            options.SuggestedFileName = browserConfig.SuggestedFileName;
        }
        
        IReadOnlyList<IStorageFolder> result = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(options);

        if (result is not [IStorageFolder target] || target.TryGetLocalPath() is not string path) {
            return;
        }

        Target = path;
    }

    partial void OnTargetChanged(string? value)
    {
        _context.Properties[_propertyInfo.Name]
            .Property
            .SetValue(_context, (FileOrFolder)value);
    }
    
    private static FilePickerFileType[] ParseFilterString(string? filter = null)
    {
        if (filter != null) {
            try {
                string[] groups = filter.Split('|');
                var types = new FilePickerFileType[groups.Length];

                for (int i = 0; i < groups.Length; i++) {
                    string[] pair = groups[i].Split(':');
                    types[i] = new FilePickerFileType(pair[0]) {
                        Patterns = pair[1].Split(';')
                    };
                }

                return types;
            }
            catch {
                throw new FormatException(
                    $"Could not parse filter arguments '{filter}'.\n" +
                    $"Example: \"Yaml Files:*.yml;*.yaml|All Files:*.*\"."
                );
            }
        }

        return [];
    }
}