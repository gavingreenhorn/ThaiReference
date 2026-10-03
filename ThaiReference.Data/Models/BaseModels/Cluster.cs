using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models.BaseModels;

public class Cluster : ICluster
{
    public string Type { get; set; } = "";
    public string? Subtype { get; set; }
    public string Value { get; set; } = "";
    public IEnumerable<IWord> SampleWords { get; set; } = [];
}
