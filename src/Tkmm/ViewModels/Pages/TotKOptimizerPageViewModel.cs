using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Core;

namespace Tkmm.ViewModels.Pages;

public partial class TotKOptimizerPageViewModel : ObservableObject
{
    private const string ConfigFilePath = "romfs/UltraCam/maxlastbreath.ini";
    private readonly ObservableCollection<OptionModel> _options;

    public TotKOptimizerPageViewModel()
    {
        _options = LoadOptions();
        GenerateConfigCommand = new RelayCommand(async () => await GenerateConfigFile());
    }

    public ObservableCollection<OptionModel> Options => _options;
    public IEnumerable<OptionModel> MainOptions =>
        Options.Where(o => o.Section?.ToLowerInvariant() == "main" && !o.Auto);
    public IEnumerable<OptionModel> ExtrasOptions =>
        Options.Where(o => o.Section?.ToLowerInvariant() == "extra" && !o.Auto);

    public IRelayCommand GenerateConfigCommand { get; }

    public async Task GenerateConfigFile()
    {
        string outputPath = Path.Combine(TKMM.MergedOutputFolder, ConfigFilePath);
        
        string? directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        string configContent = GenerateConfigContent();
        
        await File.WriteAllTextAsync(outputPath, configContent);
    }

    private string GenerateConfigContent()
    {
        var sectionLines = new Dictionary<string, List<string>>();

        foreach (var option in Options)
        {
            if (option.ConfigClass == null || option.ConfigClass.Count < 2)
                continue;

            string section = option.ConfigClass[0];
            if (!sectionLines.ContainsKey(section))
            {
                sectionLines[section] = new List<string>();
            }

            if (option.ConfigClass.Count == 2)
            {
                string key = option.ConfigClass[1];
                string value = FormatOptionValue(option);
                sectionLines[section].Add($"{key} = {value}");
            }
            else if (option.ConfigClass.Count == 3)
            {
                string key1 = option.ConfigClass[1];
                string key2 = option.ConfigClass[2];
                if (option.SelectedValue.Contains("x"))
                {
                    var parts = option.SelectedValue.Split('x');
                    if (parts.Length >= 2)
                    {
                        sectionLines[section].Add($"{key1} = {parts[0]}");
                        sectionLines[section].Add($"{key2} = {parts[1]}");
                    }
                }
                else
                {
                    sectionLines[section].Add($"{key1} = {option.SelectedValue}");
                }
            }
        }

        var sb = new StringBuilder();
        foreach (var pair in sectionLines)
        {
            sb.AppendLine($"[{pair.Key}]");
            foreach (var line in pair.Value)
            {
                sb.AppendLine(line);
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string FormatOptionValue(OptionModel option)
    {
        if (option.Class.ToLowerInvariant() == "bool")
        {
            return option.SelectedValue.ToLowerInvariant() == "true" ? "On" : "Off";
        }
        else
        {
            return option.SelectedValue;
        }
    }

    private ObservableCollection<OptionModel> LoadOptions()
    {
        var options = new ObservableCollection<OptionModel>();
        string resourcePath = "Tkmm.Resources.Optimizer.Options.json";

        using Stream? stream = typeof(TotKOptimizerPageViewModel).Assembly.GetManifestResourceStream(resourcePath);
        if (stream is null)
        {
            throw new FileNotFoundException("Options.json not found.");
        }

        using StreamReader reader = new(stream);
        string json = reader.ReadToEnd();
        var optionsData = JsonDocument.Parse(json).RootElement.GetProperty("Keys");

        foreach (var option in optionsData.EnumerateObject())
        {
            string name = option.Value.GetProperty("Name").GetString();
            string classType = option.Value.GetProperty("Class").GetString();
            string section = option.Value.GetProperty("Section").GetString();

            JsonElement defaultElem = option.Value.GetProperty("Default");
            string defaultValue;
            if (defaultElem.ValueKind == JsonValueKind.Number)
            {
                defaultValue = defaultElem.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                defaultValue = defaultElem.ToString();
            }

            List<string> values;
            if (option.Value.TryGetProperty("Values", out JsonElement valuesElem) &&
                valuesElem.ValueKind == JsonValueKind.Array)
            {
                values = valuesElem.EnumerateArray().Select(v => v.ToString()).ToList();
            }
            else
            {
                values = new List<string>();
            }

            List<string> nameValues = new List<string>();
            if (option.Value.TryGetProperty("Name_Values", out JsonElement nameValuesElem) &&
                nameValuesElem.ValueKind == JsonValueKind.Array)
            {
                nameValues = nameValuesElem.EnumerateArray().Select(e => e.ToString()).ToList();
            }

            string selectedValue = defaultValue;
            int dropdownIndex = 0;
            if (classType.ToLowerInvariant() == "dropdown")
            {
                if (defaultElem.ValueKind == JsonValueKind.Number && int.TryParse(defaultElem.GetRawText(), out int index))
                {
                    dropdownIndex = index;
                    selectedValue = (index >= 0 && index < values.Count) ? values[index] : defaultValue;
                }
                else
                {
                    selectedValue = (values.Count > 0) ? values[0] : defaultValue;
                }
            }
            else
            {
                selectedValue = defaultValue;
            }

            double increments = 1.0;
            if (option.Value.TryGetProperty("Increments", out JsonElement incElem) &&
                incElem.ValueKind == JsonValueKind.Number)
            {
                increments = incElem.GetDouble();
            }

            List<string> configClass = new List<string>();
            if (option.Value.TryGetProperty("Config_Class", out JsonElement configClassElem) &&
                configClassElem.ValueKind == JsonValueKind.Array)
            {
                configClass = configClassElem.EnumerateArray().Select(e => e.ToString()).ToList();
            }

            bool auto = false;
            if (option.Value.TryGetProperty("Auto", out JsonElement autoElem) && autoElem.ValueKind == JsonValueKind.True)
            {
                auto = true;
            }
            else if (option.Value.TryGetProperty("Auto", out autoElem) && autoElem.ValueKind == JsonValueKind.False)
            {
                auto = false;
            }

            options.Add(new OptionModel
            {
                Name = name,
                DefaultValue = defaultValue,
                Values = values,
                NameValues = nameValues,
                SelectedValue = selectedValue,
                SelectedIndex = (classType.ToLowerInvariant() == "dropdown") ? dropdownIndex : 0,
                Class = classType,
                Section = section,
                Increments = increments,
                ConfigClass = configClass,
                Auto = auto
            });
        }

        return options;
    }
}

public class OptionModel : ObservableObject
{
    public string Name { get; set; }
    public string DefaultValue { get; set; }
    public List<string> Values { get; set; }
    public List<string> NameValues { get; set; }

    private string selectedValue;
    public string SelectedValue
    {
        get => selectedValue;
        set => SetProperty(ref selectedValue, value);
    }

    private int selectedIndex;
    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if (SetProperty(ref selectedIndex, value))
            {
                if (Class != null && Class.ToLowerInvariant() == "dropdown" && value >= 0 && value < Values.Count)
                {
                    SelectedValue = Values[value];
                    OnPropertyChanged(nameof(DisplaySelectedValue));
                }
            }
        }
    }

    public string DisplaySelectedValue =>
        (Class?.ToLowerInvariant() == "dropdown" && NameValues != null && NameValues.Count > SelectedIndex)
            ? NameValues[SelectedIndex]
            : SelectedValue;

    public string Class { get; set; }
    public string Section { get; set; }
    public double Increments { get; set; }
    public List<string> ConfigClass { get; set; }
    public bool Auto { get; set; }

    public IEnumerable<string> DisplayValues => (NameValues?.Count ?? 0) > 0 ? NameValues : Values;
}