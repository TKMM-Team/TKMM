using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Tkmm.Converters;

public class SemicolonDelimitedListConverter : IValueConverter
{
    public static SemicolonDelimitedListConverter Shared { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IEnumerable<string> collection) {
            return string.Join(';', collection);
        }

        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text) {
            return new ObservableCollection<string>(text.Split(';'));
        }

        return new ObservableCollection<string>();
    }
}
