using System.Globalization;
using ThaiReference.Ui;

namespace ThaiReference.Converters;

/// <summary>
/// MultiBinding: [0] gloss string (e.g. EnglishLabel), [1] <see cref="UiLanguage"/>.
/// Visible only when UI is English and gloss is non-empty.
/// </summary>
public sealed class EnglishGlossVisibleConverter : IMultiValueConverter
{
	public object? Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
	{
		if (values is null || values.Length < 2)
			return false;
		if (values[1] is not UiLanguage lang || lang != UiLanguage.English)
			return false;
		return values[0] is string s && !string.IsNullOrWhiteSpace(s);
	}

	public object[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
		throw new NotSupportedException();
}
