using System.ComponentModel;
using ThaiReference.Utilities;
using ThaiReference.ViewModels;
using ThaiReference.Views.Drawables;

namespace ThaiReference.Views;

public partial class MonthsView : ContentPage
{
    private readonly ThaiMonthsDrawable _drawable = new();
    private MonthsViewModel ViewModel => (MonthsViewModel)BindingContext;

    public MonthsView(MonthsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        MonthsWheel.Drawable = _drawable;
        _drawable.BackdropColor = ThemeColors.Get("MonthWheelBackdrop");
        _drawable.CenterFillColor = ThemeColors.Get("SurfaceInsetDark");
        _drawable.CenterStrokeColor = ThemeColors.Get("MonthWheelCenterStroke");
        _drawable.ZodiacSymbolColor = ThemeColors.Get("MonthWheelZodiacSymbol");
        _drawable.SelectionStrokeColor = ThemeColors.Get("MonthWheelSelectionStroke");
        _drawable.Configure(vm.Months);
        vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MonthsViewModel.SelectedWheelItem))
        {
            _drawable.SelectedOrdinal = ViewModel.SelectedWheelItem?.Ordinal ?? -1;
            MonthsWheel.Invalidate();
        }
    }

    private void OnWheelTapped(object sender, TouchEventArgs e)
    {
        var touch = e.Touches[0];
        int ordinal = _drawable.HitTest(touch.X, touch.Y);
        if (ordinal < 0) return;

        ViewModel.SelectMonthCommand.Execute(ordinal);
    }
}
