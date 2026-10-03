using ThaiReference.Models;
using ThaiReference.Views.Drawables;

namespace ThaiReference.Views.Controls;

public class ClusterAnatomyGraphicsView : GraphicsView
{
    public static readonly BindableProperty AnatomyTypeProperty = BindableProperty.Create(
        nameof(AnatomyType),
        typeof(ClusterAnatomyType),
        typeof(ClusterAnatomyGraphicsView),
        ClusterAnatomyType.True,
        propertyChanged: OnAnatomyTypeChanged);

    public ClusterAnatomyType AnatomyType
    {
        get => (ClusterAnatomyType)GetValue(AnatomyTypeProperty);
        set => SetValue(AnatomyTypeProperty, value);
    }

    public ClusterAnatomyGraphicsView()
    {
        ApplyDrawable(AnatomyType);
    }

    private static void OnAnatomyTypeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ClusterAnatomyGraphicsView view && newValue is ClusterAnatomyType type)
            view.ApplyDrawable(type);
    }

    private void ApplyDrawable(ClusterAnatomyType type) =>
        Drawable = new ClusterAnatomyDrawable { ClusterType = type };
}
