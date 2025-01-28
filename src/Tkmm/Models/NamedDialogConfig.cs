using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Data;

namespace Tkmm.Models;

public class NamedDialogConfig : ObservableCollection<NamedDialog>
{
    public static NamedDialogConfig Shared { get; } = Load();

    private NamedDialogConfig()
    {
    }

    public NamedDialog this[string key] {
        get {
            if (this.FirstOrDefault(namedDialog => namedDialog.Name == key) is not NamedDialog dialog) {
                Add(dialog = new NamedDialog(key, false));
            }

            return dialog;
        }
    }

    public static Binding GetBinding(string key)
    {
        return new Binding($"[{key}].IsSuppressed");
    }

    public static NamedDialogConfig Load()
    {
        string configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".config", "dialogs.json");

        if (!File.Exists(configFile)) {
            return [];
        }
        
        using Stream stream = File.OpenRead(configFile);
        return JsonSerializer.Deserialize(stream, NamedDialogConfigJsonContext.Default.NamedDialogConfig)
            ?? [];
    }
}

[JsonSerializable(typeof(NamedDialogConfig))]
public partial class NamedDialogConfigJsonContext : JsonSerializerContext;