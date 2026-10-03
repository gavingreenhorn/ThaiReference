using ThaiReference.Models;
using System.Globalization;

namespace ThaiReference.Converters;

public class GramPersonLookupConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if ((values?.Length) == 2 && values[0] is ILookup<int, PronounItem> lookup && values[1] is int ix)
        {
            var group = lookup[ix].Concat(lookup[0]);

            return group.Any() ? group : null;
        }

        return null;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
