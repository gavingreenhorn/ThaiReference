using System.Globalization;

namespace ThaiReference.Converters;

/// <summary>Maps a hex string (e.g. <c>#eab7b7</c>) to a <see cref="Color"/>; invalid/null → Transparent.</summary>
public sealed class HexToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string hex || string.IsNullOrWhiteSpace(hex))
            return Colors.Transparent;
        try
        {
            return Color.FromArgb(hex);
        }
        catch
        {
            return Colors.Transparent;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
