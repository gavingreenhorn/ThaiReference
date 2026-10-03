using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models;

public class HueFamily : IHueFamily
{
    public int Id { get; set; }
    public string ThaiName { get; set; } = string.Empty;
    public string EnglishName { get; set; } = string.Empty;
    public string Hex { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public IReadOnlyList<IColor> Colors { get; set; } = [];
}
