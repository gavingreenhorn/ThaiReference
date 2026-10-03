using System.ComponentModel;
using ThaiReference.Utilities;
using ThaiReference.ViewModels;
using ThaiReference.Views.Drawables;

namespace ThaiReference.Views
{
    public partial class ThaiClockView : ContentPage
    {
        private readonly ThaiClockDrawable _drawable = new();
        private ThaiClockViewModel ViewModel => (ThaiClockViewModel)BindingContext;

        public ThaiClockView(ThaiClockViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;

            ClockView.Drawable = _drawable;
            ApplyThemeColors();
            _drawable.Configure(vm.TimeOfDaySegments);
            _drawable.IsCenterSelected = vm.IsCenterSelected;
            vm.PropertyChanged += OnViewModelPropertyChanged;
        }

        private void ApplyThemeColors()
        {
            _drawable.BackdropColor = ThemeColors.Get("ClockBackdrop");
            _drawable.CenterFillColor = ThemeColors.Get("ClockLcdPanel");
            _drawable.CenterStrokeColor = ThemeColors.Get("ClockCenterStroke");
            _drawable.SelectionStrokeColor = ThemeColors.Get("ClockSelectionStroke");
            _drawable.DayColor = ThemeColors.Get("ClockDay");
            _drawable.NightColor = ThemeColors.Get("ClockNight");
            _drawable.InnerFallbackColor = ThemeColors.Get("ClockInnerFallback");
        }

        private void OnClockSizeChanged(object? sender, EventArgs e)
        {
            double min = Math.Min(ClockView.Width, ClockView.Height);
            if (min <= 0) return;
            double hubDiameter = min * 0.34;
            const double DsegEmWidth = 3.464;
            double usable = hubDiameter * 0.88;
            double lcd = Math.Max(12, usable / DsegEmWidth);
            LcdTimeLabel.FontSize = lcd;
            LcdGhostLabel.FontSize = lcd;
            LcdHost.WidthRequest = lcd * DsegEmWidth;
            LcdHost.HeightRequest = lcd * 1.25;
        }

        private IDispatcherTimer? _timer;

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.SelectCurrentHourCommand.Execute(null);
            _drawable.SelectedOuterOrdinal = ViewModel.SelectedOuterOrdinal;
            _drawable.SelectedInnerHour = ViewModel.SelectedInnerHour;
            _drawable.SelectedDayNightSegment = ViewModel.SelectedDayNightSegment;
            _drawable.IsCenterSelected = ViewModel.IsCenterSelected;
            ClockView.Invalidate();

            _timer ??= Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick -= OnTimerTick;
            _timer.Tick += OnTimerTick;
            ViewModel.Tick();
            _timer.Start();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _timer?.Stop();
        }

        private void OnTimerTick(object? sender, EventArgs e) => ViewModel.Tick();

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ThaiClockViewModel.SelectedOuterOrdinal)
                              or nameof(ThaiClockViewModel.SelectedInnerHour)
                              or nameof(ThaiClockViewModel.SelectedDayNightSegment)
                              or nameof(ThaiClockViewModel.IsCenterSelected))
            {
                _drawable.SelectedOuterOrdinal = ViewModel.SelectedOuterOrdinal;
                _drawable.SelectedInnerHour = ViewModel.SelectedInnerHour;
                _drawable.SelectedDayNightSegment = ViewModel.SelectedDayNightSegment;
                _drawable.IsCenterSelected = ViewModel.IsCenterSelected;
                ClockView.Invalidate();
            }
        }

        private void OnClockTapped(object sender, TouchEventArgs e)
        {
            var touch = e.Touches[0];
            var hit = _drawable.HitTest(touch.X, touch.Y);
            if (hit.IsNone) return;

            switch (hit.Ring)
            {
                case ThaiClockDrawable.HitRing.Center:
                    ViewModel.SelectCenterCommand.Execute(null);
                    break;
                case ThaiClockDrawable.HitRing.DayNight:
                    ViewModel.SelectDayNightCommand.Execute(hit.Index);
                    break;
                case ThaiClockDrawable.HitRing.Outer:
                    ViewModel.SelectTimeOfDayCommand.Execute(hit.Index);
                    break;
                default:
                    ViewModel.SelectHourCommand.Execute(hit.Index);
                    break;
            }
        }
    }
}
