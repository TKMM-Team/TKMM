using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using TkSharp.Core.Models;

namespace Tkmm.Converters;

public class PathToThumbnailConverter : IValueConverter
{
    public static readonly PathToThumbnailConverter Instance = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not TkThumbnail thumbnail ? null : thumbnail.ThumbnailPath;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string path || !File.Exists(path)) {
            return null;
        }

        return new TkThumbnail {
            ThumbnailPath = path,
            Bitmap = new Bitmap(path)
        };
    }
}