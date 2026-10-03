using System.Globalization;

namespace ThaiReference.Converters;

public class RelativeWidthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return null;
        if (value is double parentWidth && parameter is double rate)
            return parentWidth * rate;
        throw new InvalidCastException("RelativeWidthConverter received incorrect input value type");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
