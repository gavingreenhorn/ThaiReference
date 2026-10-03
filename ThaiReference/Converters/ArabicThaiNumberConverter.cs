using System.Globalization;
using ThaiReference.Models;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Converters
{
    public class ArabicThaiNumberConverter(INumeral[] numerals) : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not int arabic) return value;
            return new ThaiNumber(arabic, numerals).Content;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
