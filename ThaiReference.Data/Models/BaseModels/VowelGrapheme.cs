using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models.BaseModels;

public class VowelGrapheme : IVowelGrapheme
{
    public string Value { get; set; } = "";
    public bool IsClosed { get; set; }
}
