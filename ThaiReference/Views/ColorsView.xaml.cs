using System.ComponentModel;
using ThaiReference.ViewModels;
using ThaiReference.Views.Drawables;

namespace ThaiReference.Views;

public partial class ColorsView : ContentPage
{
    private readonly ColorWheelDrawable _drawable = new();
    ColorsViewModel ViewModel => (ColorsViewModel)BindingContext;

    public ColorsView(ColorsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.PropertyChanged += OnViewModelPropertyChanged;

        _drawable.Configure(vm.HueFamilies);
        ColorWheel.Drawable = _drawable;

        var tap = new TapGestureRecognizer();
        tap.Tapped += OnColorWheelTapped;
        ColorWheel.GestureRecognizers.Add(tap);

        if (Application.Current is not null)
            Application.Current.RequestedThemeChanged += OnThemeChanged;

        ApplyTheme();
    }

    void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) =>
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ApplyTheme();
            ColorWheel.Invalidate();
        });

    void ApplyTheme()
    {
        bool dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        _drawable.BackdropColor = dark ? Color.FromArgb("#1E1E1E") : Color.FromArgb("#F5F5F5");
        _drawable.SelectionStrokeColor = dark ? Colors.White : Color.FromArgb("#333333");
    }

    void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ColorsViewModel.SelectedColor))
        {
            _drawable.SelectedColor = ViewModel.SelectedColor;
            ColorWheel.Invalidate();
        }
        else if (e.PropertyName == nameof(ColorsViewModel.SelectedFamily))
        {
            _drawable.SelectedFamily = ViewModel.SelectedFamily;
            ColorWheel.Invalidate();
        }
    }

    void OnColorWheelTapped(object? sender, TappedEventArgs e)
    {
        var pos = e.GetPosition(ColorWheel);
        if (pos is null) return;

        var hit = _drawable.HitTest((float)pos.Value.X, (float)pos.Value.Y);
        if (hit.IsNone) return;

        switch (hit.Type)
        {
            case ColorWheelDrawable.HitType.Center:
                if (ViewModel.SelectedFamily != null)
                {
                    ViewModel.SelectFamilyCommand.Execute(null);
                    ViewModel.SelectColorCommand.Execute(null);
                }
                break;
            case ColorWheelDrawable.HitType.Family:
                ViewModel.SelectFamilyCommand.Execute(hit.Family);
                break;
            case ColorWheelDrawable.HitType.Color:
                ViewModel.SelectColorCommand.Execute(hit.Color);
                break;
        }
    }
}
