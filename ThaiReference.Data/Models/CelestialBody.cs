using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models;

public class CelestialBody : ICelestialBody
{
    public int Ordinal { get; set; }

    public string Glyph { get; set; } = "";

    public string ColorHex { get; set; } = "";

    public IWord Word { get; set; } = null!;
}
