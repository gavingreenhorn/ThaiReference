using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models;

public class TimeOfDay : ITimeOfDay
{
    public int Ordinal { get; set; }
    public IWord Word { get; set; } = null!;

    public string Audible => Word?.Value ?? string.Empty;
}
