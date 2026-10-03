namespace ThaiReference.Utilities;

/// <summary>Reads colors from <c>Resources/Styles/Colors.xaml</c>.</summary>
public static class ThemeColors
{
    public static Color Get(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color)
            return color;
        return Colors.Transparent;
    }

    public static string ToHex(Color color) =>
        $"#{(byte)(color.Red * 255):X2}{(byte)(color.Green * 255):X2}{(byte)(color.Blue * 255):X2}";
}
