using ConfigFactory;
using ConfigFactory.Core;
using ConfigFactory.Core.Models;
using ConfigFactory.Models;
using TkSharp.Core;

namespace Tkmm.Extensions;

public static class ConfigFactoryExtensions
{
    public static void AppendAndValidate<T>(this ConfigPageModel settingsModel, ref bool isValid) where T : ConfigModule<T>, new()
    {
        isValid = ConfigModule<T>.Shared.Validate(out string? _, out ConfigProperty? target);
        
        if (!isValid && target?.Attribute is not null) {
            settingsModel.SelectedGroup = settingsModel.Categories
                .Where(x => x.Header == target.Attribute.Category)
                .SelectMany(x => x.Groups)
                .FirstOrDefault(x => x.Header == target.Attribute.Group);

            TkStatus.Set(Locale[TkLocale.Error_InvalidSetting, target.Property.Name], TkIcons.WARNING);
        }

        settingsModel.Append<T>();
    }
}