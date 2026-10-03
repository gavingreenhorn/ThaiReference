using System.Globalization;

namespace ThaiReference.Converters;

/// <summary>
/// Font family for flyout glyphs keyed by <c>Route</c> / menu <c>ClassId</c>.
/// </summary>
public sealed class ShellNavGlyphFontConverter : IValueConverter
{
	public const string IconsSharp = "MaterialIconsSharp";
	public const string IconsSharpAndroid = "MaterialIconsSharp-Regular";
	public const string Default = "OpenSans";

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		var key = value?.ToString()?.Trim();
		if (key is not ("time" or "settings" or "space" or "orphography"))
			return Default;

		return DeviceInfo.Platform == DevicePlatform.Android ? IconsSharpAndroid : IconsSharp;
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		throw new NotSupportedException();
}
