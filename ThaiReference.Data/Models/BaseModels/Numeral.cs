using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models.BaseModels;

public class Numeral : INumeral
{
    public IWord Name { get; set; } = null!;
    public string Grapheme { get; set; } = "";

    public Numeral() { }
    public Numeral(string grapheme, IWord name)
    {
        Grapheme = grapheme;
        Name = name;
    }
}
