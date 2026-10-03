using System.Globalization;
using ThaiReference.Models.Interfaces;
using ThaiReference.Ui;

namespace ThaiReference.Converters;

/// <summary>
/// MultiBinding: [0] <see cref="IColor"/>, [1] <see cref="UiLanguage"/>.
/// Thai UI → spelling from words; English UI → <see cref="IColor.EnglishLabel"/>.
/// </summary>
public sealed class LocalizedColorLabelConverter : IMultiValueConverter
{
	public object? Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
	{
		if (values is null || values.Length < 2 || values[0] is not IColor color)
			return string.Empty;
		var lang = values[1] is UiLanguage u ? u : UiLanguageHelper.Read();
		if (lang == UiLanguage.Thai)
		{
			var spelling = color.Spelling;
			return string.IsNullOrWhiteSpace(spelling) ? color.EnglishLabel : spelling;
		}
		return color.EnglishLabel;
	}

	public object[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
		throw new NotSupportedException();
}
