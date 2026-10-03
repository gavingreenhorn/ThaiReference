using System.Collections;
using System.Globalization;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Converters;

/// <summary>Bool: binding value is null.</summary>
public sealed class IsNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

/// <summary>Bool: binding value is not null.</summary>
public sealed class IsNotNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

/// <summary>
/// Bool: enumerable has at least one item. Supports <see cref="ICollection"/> and <see cref="IEnumerable{T}"/> of <see cref="IWord"/>.
/// Set <paramref name="parameter"/> to <c>Invert</c> for the opposite (e.g. show a label only when there are no word rows).
/// </summary>
public sealed class EnumerableAnyToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool any = value switch
        {
            null => false,
            ICollection c => c.Count > 0,
            IEnumerable<IWord> w => w.Any(),
            IEnumerable e => e.Cast<object?>().Any(x => x is not null),
            _ => false
        };

        return string.Equals(parameter?.ToString(), "Invert", StringComparison.OrdinalIgnoreCase) ? !any : any;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

/// <summary>Bool: string is non-null and not whitespace (for conditional visibility).</summary>
public sealed class IsNonEmptyStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string s ? !string.IsNullOrWhiteSpace(s) : false;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
