using System.Globalization;

namespace ThaiReference.Converters;

/// <summary>
/// Maps flyout <see cref="Microsoft.Maui.Controls.Shell"/> item <c>Route</c> / menu <c>ClassId</c> to a display glyph.
/// </summary>
public sealed class ShellNavGlyphConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		var key = value?.ToString()?.Trim();
		var scope = parameter?.ToString();

		if (string.Equals(scope, "MenuItem", StringComparison.OrdinalIgnoreCase))
		{
			return key switch
			{
				"settings" => "\ue8b8", // settings (Material Icons Sharp)
				"help" => "?",
				"about" => "\u2139",
				_ => "\u2022",
			};
		}

		return key switch
		{
			"orphography" => "\ue262", // text_fields
			"grammar" => "\u00A7",
			"terms" => "\u203B",
			"space" => "\ue55c", // my_location
			"time" => "\ue88b", // hourglass_empty
			"homophones" => "\u2248",
			"minimalsets" => "\u2260",
			_ => "\u25C6",
		};
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		throw new NotSupportedException();
}
