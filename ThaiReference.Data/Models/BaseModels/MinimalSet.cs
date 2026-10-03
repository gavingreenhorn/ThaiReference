using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models.BaseModels;

public class MinimalSet : IMinimalSet
{
    public string Label { get; set; } = "";
    public IEnumerable<IWord> Words { get; set; } = [];
}
