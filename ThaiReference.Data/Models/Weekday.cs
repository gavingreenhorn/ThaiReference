using CommunityToolkit.Mvvm.ComponentModel;
using ThaiReference.Contracts;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models;

public partial class Weekday : ObservableObject, IWeekday, IHaveDescription
{
    public int Ordinal { get; set; }

    public IWord Word { get; set; } = null!;

    public string? Deity { get; set; }

    public string? ImageUrl { get; set; }

    public string Audible => "วัน" + Word.Value;

    [ObservableProperty]
    private bool isVisible = true;
}
