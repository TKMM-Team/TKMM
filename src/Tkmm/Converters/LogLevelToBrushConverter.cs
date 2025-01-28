using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Microsoft.Extensions.Logging;

namespace Tkmm.Converters;

public class LogLevelToBrushConverter : IValueConverter
{
    private static readonly IBrush _default = Brushes.Transparent;
    private static readonly IBrush _info = Brush.Parse("#2E5FC9");
    private static readonly IBrush _debug = Brush.Parse("#E3BA5B");
    private static readonly IBrush _warning = Brush.Parse("#C9962E");
    private static readonly IBrush _error = Brush.Parse("#C9402E");

    public static LogLevelToBrushConverter Shared { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) 
        => value switch {
            LogLevel.Debug => _debug,
            LogLevel.Information => _info,
            LogLevel.Warning => _warning,
            LogLevel.Error => _error,
            _ => _default
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (_info.Equals(value)) {
            return LogLevel.Information;
        }
        
        if (_debug.Equals(value)) {
            return LogLevel.Debug;
        }
        
        if (_warning.Equals(value)) {
            return LogLevel.Warning;
        }
        
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (_error.Equals(value)) {
            return LogLevel.Error;
        }

        return LogLevel.None;
    }
}