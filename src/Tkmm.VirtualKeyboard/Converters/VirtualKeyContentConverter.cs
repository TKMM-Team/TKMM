using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.VisualTree;

namespace Tkmm.VirtualKeyboard.Converters;

public sealed class VirtualKeyContentConverter : IValueConverter
{
    public static readonly VirtualKeyContentConverter Instance = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not VirtualKey key) {
            return null;
        }

        if (key.Content is not null) {
            return key.Content;
        }

        if (key.FindAncestorOfType<VirtualKeyboardLayout>() is not VirtualKeyboardLayout layout) {
            return key.Content = key.Key;
        }

        layout.RegisterKey(key);
        
        return key.Content = layout.IsShiftEnabled ? key.ShiftKey : key.Key;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}