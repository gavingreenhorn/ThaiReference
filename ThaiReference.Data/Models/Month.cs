using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models;

public class Month : IMonth
{
    public int Ordinal { get; set; }
    public IWord Word { get; set; } = null!;
    public string ShortName { get; set; } = string.Empty;
    public string EnglishName { get; set; } = string.Empty;
    public IZodiac Zodiac { get; set; } = null!;

    public string Audible => "เดือน" + (Word?.Value ?? string.Empty);
}
