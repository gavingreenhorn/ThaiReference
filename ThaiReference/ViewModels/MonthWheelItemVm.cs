using ThaiReference.Models.Interfaces;

namespace ThaiReference.ViewModels;

/// <summary>One month on the wheel: domain + strip rendering fields (built in <see cref="MonthsViewModel"/>).</summary>
public sealed class MonthWheelItemVm
{
    public IMonth Month { get; }
    public string ShortName { get; }
    public string ShortEnglish { get; }
    public string ZodiacSymbol { get; }
    public string ElementLightHex { get; }
    public string ElementDarkHex { get; }
    public int Ordinal => Month.Ordinal;
    public int SeasonOrdinal { get; }

    public MonthWheelItemVm(
        IMonth month,
        int seasonOrdinal,
        string shortName,
        string shortEnglish,
        string zodiacSymbol,
        string elementLightHex,
        string elementDarkHex)
    {
        Month = month;
        SeasonOrdinal = seasonOrdinal;
        ShortName = shortName;
        ShortEnglish = shortEnglish;
        ZodiacSymbol = zodiacSymbol;
        ElementLightHex = elementLightHex;
        ElementDarkHex = elementDarkHex;
    }
}
