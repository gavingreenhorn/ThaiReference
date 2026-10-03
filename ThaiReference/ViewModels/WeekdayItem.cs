using CommunityToolkit.Mvvm.ComponentModel;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.ViewModels;

public sealed partial class WeekdayItem(IWeekday weekday, ICelestialBody celestial) : ObservableObject
{
    public IWeekday Weekday { get; } = weekday;

    public ICelestialBody Celestial { get; } = celestial;

    public Color Background => Color.FromArgb(Celestial.ColorHex);

    public Color HaloFill => Background.WithAlpha(0.38f);

    [ObservableProperty]
    private bool isSelected;
}
