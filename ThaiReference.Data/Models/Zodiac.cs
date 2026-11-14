using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models;

public class Zodiac : IZodiac
{
    public int Ordinal { get; set; }
    public string EnglishName { get; set; } = string.Empty;
    public string SanskritName { get; set; } = string.Empty;
    public string SanskritLexeme { get; set; } = string.Empty;
    public string Element { get; set; } = string.Empty;
    public string Glyph { get; set; } = string.Empty;
}
