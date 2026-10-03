using ThaiReference.Contracts;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models.BaseModels;

public class Mark : IMark
{
    public string Grapheme { get; set; } = "";
    public string Name => string.Join(" ", Words.Select(x => x.Value));
    public string? EnglishLabel { get; set; }
    public bool ToneMark { get; set; }
    public byte[]? Media { get; set; }
    public IEnumerable<IWord> Words { get; set; } = [];

    string IAudible.Audible => Name;
}
