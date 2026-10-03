using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Converters;

/// <summary><see cref="IMark.Name"/> + word romanizations for the active option, one string.</summary>
public sealed class MarkLineConverter : IMultiValueConverter
{
    public object? Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Length < 2)
            return string.Empty;

        if (values[0] is not IMark mark)
            return string.Empty;

        var option = values[1]?.ToString();
        if (string.IsNullOrWhiteSpace(option))
            option = "IPA";

        var name = mark.Name?.Trim() ?? string.Empty;
        var romanParts = new List<string>();
        foreach (var w in mark.Words)
        {
            var t = w.Transcription;
            if (t is null)
                continue;
            var p = t.GetType().GetProperty(
                option,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var s = p?.GetValue(t)?.ToString();
            if (!string.IsNullOrWhiteSpace(s))
                romanParts.Add(s.Trim());
        }

        var roman = string.Join(" ", romanParts);
        if (string.IsNullOrEmpty(name))
            return roman;
        if (string.IsNullOrEmpty(roman))
            return name;
        return $"{name} · {roman}";
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
