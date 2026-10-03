using ThaiReference.Models;

namespace ThaiReference.Views.Controls;

/// <summary>
/// Hosts pre-built anatomy strips; shows one at a time when <see cref="ActiveAnatomyType"/> changes.
/// </summary>
public class ClusterAnatomyStripHost : Grid
{
    public static readonly BindableProperty ActiveAnatomyTypeProperty = BindableProperty.Create(
        nameof(ActiveAnatomyType),
        typeof(ClusterAnatomyType),
        typeof(ClusterAnatomyStripHost),
        ClusterAnatomyType.True,
        propertyChanged: OnActiveAnatomyTypeChanged);

    private readonly ClusterAnatomyGraphicsView _trueStrip;
    private readonly ClusterAnatomyGraphicsView _falseStrip;
    private readonly ClusterAnatomyGraphicsView _leadingStrip;
    private readonly ClusterAnatomyGraphicsView _enepentheticStrip;
    private readonly ClusterAnatomyGraphicsView _nonSonorantStrip;
    private readonly ClusterAnatomyGraphicsView _subSyllableStrip;

    public ClusterAnatomyType ActiveAnatomyType
    {
        get => (ClusterAnatomyType)GetValue(ActiveAnatomyTypeProperty);
        set => SetValue(ActiveAnatomyTypeProperty, value);
    }

    public ClusterAnatomyStripHost()
    {
        _trueStrip = CreateStrip(ClusterAnatomyType.True);
        _falseStrip = CreateStrip(ClusterAnatomyType.False);
        _leadingStrip = CreateStrip(ClusterAnatomyType.Leading);
        _enepentheticStrip = CreateStrip(ClusterAnatomyType.Enepenthetic);
        _nonSonorantStrip = CreateStrip(ClusterAnatomyType.NonSonorant);
        _subSyllableStrip = CreateStrip(ClusterAnatomyType.SubSyllable);

        Children.Add(_trueStrip);
        Children.Add(_falseStrip);
        Children.Add(_leadingStrip);
        Children.Add(_enepentheticStrip);
        Children.Add(_nonSonorantStrip);
        Children.Add(_subSyllableStrip);

        Show(ClusterAnatomyType.True);
    }

    private static ClusterAnatomyGraphicsView CreateStrip(ClusterAnatomyType type) =>
        new()
        {
            AnatomyType = type,
            HeightRequest = 92,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
        };

    private static void OnActiveAnatomyTypeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ClusterAnatomyStripHost host && newValue is ClusterAnatomyType type)
            host.Show(type);
    }

    private void Show(ClusterAnatomyType type)
    {
        _trueStrip.IsVisible = type == ClusterAnatomyType.True;
        _falseStrip.IsVisible = type == ClusterAnatomyType.False;
        _leadingStrip.IsVisible = type == ClusterAnatomyType.Leading;
        _enepentheticStrip.IsVisible = type == ClusterAnatomyType.Enepenthetic;
        _nonSonorantStrip.IsVisible = type == ClusterAnatomyType.NonSonorant;
        _subSyllableStrip.IsVisible = type == ClusterAnatomyType.SubSyllable;

        StripFor(type).Invalidate();
    }

    private ClusterAnatomyGraphicsView StripFor(ClusterAnatomyType type) =>
        type switch
        {
            ClusterAnatomyType.False => _falseStrip,
            ClusterAnatomyType.Leading => _leadingStrip,
            ClusterAnatomyType.Enepenthetic => _enepentheticStrip,
            ClusterAnatomyType.NonSonorant => _nonSonorantStrip,
            ClusterAnatomyType.SubSyllable => _subSyllableStrip,
            _ => _trueStrip,
        };
}
