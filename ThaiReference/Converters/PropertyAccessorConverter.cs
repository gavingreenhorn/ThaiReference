using System.Globalization;

namespace ThaiReference.Converters;

public class PropertyAccessorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is null || values.Any(x => x is null) || values.Length < 2)
            return string.Empty;

        object inputObject = values[0];
        string propertyName = values[1].ToString();

        return inputObject.GetType().GetProperty(propertyName)?.GetValue(inputObject) ?? string.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

