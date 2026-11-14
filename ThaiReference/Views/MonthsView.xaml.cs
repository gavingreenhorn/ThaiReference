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
        _drawable.Configure(vm.Seasons, vm.Months);
        vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MonthsViewModel.SelectedWheelItem) or nameof(MonthsViewModel.SelectedSeason))
        {
            _drawable.SelectedOrdinal = ViewModel.SelectedWheelItem?.Ordinal ?? -1;
            _drawable.SelectedSeasonOrdinal = ViewModel.SelectedSeason?.Ordinal ?? -1;
            MonthsWheel.Invalidate();
        }
    }

    private void OnWheelTapped(object sender, TouchEventArgs e)
    {
        var touch = e.Touches[0];
        var hit = _drawable.HitTest(touch.X, touch.Y);
        if (hit.IsNone) return;

        if (hit.IsSeason)
            ViewModel.SelectSeasonCommand.Execute(hit.Ordinal);
        else
            ViewModel.SelectMonthCommand.Execute(hit.Ordinal);
    }
}
