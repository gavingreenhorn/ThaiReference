using System.Globalization;

namespace ThaiReference.Utilities;


public class TemplateByKeyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string key && Application.Current is { } app
            && app.Resources.TryGetValue(key, out var resource))
        {
            return resource as ControlTemplate;
        }
        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
