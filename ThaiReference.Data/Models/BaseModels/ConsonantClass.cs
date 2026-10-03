using ThaiReference.Contracts;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models.BaseModels;

public class ConsonantClass : IConsonantClass
{
    public string Code { get; set; } = "";
    public string? EnglishLabel { get; set; }
    public byte[]? Media { get; set; }
    public IEnumerable<IWord> Words { get; set; } = [];

    public string Name => string.Concat(Words.Select(x => x.Value));

    string IAudible.Audible => Name;
}
