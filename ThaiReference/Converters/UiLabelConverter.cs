using System.Globalization;
using ThaiReference.Ui;

namespace ThaiReference.Converters;

/// <summary>Value = <see cref="UiLanguage"/>; ConverterParameter = label id in <see cref="UiLabels"/>.</summary>
public sealed class UiLabelConverter : IValueConverter
{
	public static UiLabelConverter Shared { get; } = new();

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		var lang = value is UiLanguage u ? u : UiLanguageHelper.Read();
		var id = parameter as string;
		return string.IsNullOrEmpty(id) ? string.Empty : UiLabels.Get(id, lang);
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		throw new NotSupportedException();
}
