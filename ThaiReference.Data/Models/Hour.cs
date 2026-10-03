using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models;

public class Hour : IHour
{
    public int Ordinal { get; set; }
    public IWord Word { get; set; } = null!;

    public string Audible => Word?.Value ?? string.Empty;
}
