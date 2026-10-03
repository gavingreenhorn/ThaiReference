using System.ComponentModel;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using ThaiReference.Models.Interfaces;
using ThaiReference.ViewModels;

namespace ThaiReference.Views;

public partial class DirectionsView : ContentPage
{
	const double Cx = 200;
	const double Cy = 200;
	const double DialSize = 400;
	const double CardinalR = 110;
	const double OrdinalR = 96;
	const double MinorR = 52;
	const double WaistR = 28;
	const double HalfSpanDeg = 11.25;
	const double InnerRingR = 82;
	const double TickInnerR = 94;
	const double TickOuterR = 104;
	const double OuterRingR = 112;
	const double LabelR = 170;
	const double LabelREastWest = 155;

	readonly Dictionary<int, Label> _labelStacks = [];
	DirectionsViewModel ViewModel => (DirectionsViewModel)BindingContext;

	public DirectionsView(DirectionsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		vm.PropertyChanged += OnViewModelPropertyChanged;
		BuildRose();
		if (Application.Current is not null)
			Application.Current.RequestedThemeChanged += OnThemeChanged;
	}

	void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) =>
		MainThread.BeginInvokeOnMainThread(BuildRose);

	void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(DirectionsViewModel.SelectedDirection))
			ApplySelectionColors();
	}

	void BuildRose()
	{
		bool dark = Application.Current?.RequestedTheme == AppTheme.Dark;
		Color ink = dark ? Color.FromArgb("#C9B8A8") : Color.FromArgb("#5C4033");
		Color darkFill = dark ? Color.FromArgb("#C9B8A8") : Color.FromArgb("#5C4033");
		Color lightFill = dark ? Color.FromArgb("#2A2826") : Colors.White;

		CompassDial.Children.Clear();
		_labelStacks.Clear();

		AddRing(InnerRingR, ink, 1.2);
		AddRing(TickInnerR, ink, 1);
		AddRing(TickOuterR, ink, 1);
		AddRing(OuterRingR, ink, 1.4);
		AddTicks(ink);

		foreach (var deg in new[] { 22.5, 67.5, 112.5, 157.5, 202.5, 247.5, 292.5, 337.5 })
			AddPoint(deg, MinorR, darkFill, lightFill, ink);

		foreach (var deg in new[] { 45.0, 135, 225, 315 })
			AddPoint(deg, OrdinalR, darkFill, lightFill, ink);

		foreach (var deg in new[] { 0.0, 90, 180, 270 })
			AddPoint(deg, CardinalR, darkFill, lightFill, ink);

		foreach (var dir in ViewModel.Directions)
		{
			bool cardinal = dir.Degrees % 90 == 0;
			AddLabel(dir, ink, cardinal ? 15 : 12);
		}

		ApplySelectionColors();
	}

	void AddRing(double radius, Color stroke, double thickness)
	{
		double d = radius * 2;
		CompassDial.Children.Add(new Ellipse
		{
			Stroke = stroke,
			StrokeThickness = thickness,
			Fill = Colors.Transparent,
		}.WithBounds(Cx - radius, Cy - radius, d, d));
	}

	void AddTicks(Color stroke)
	{
		for (int deg = 0; deg < 360; deg += 5)
		{
			bool major = deg % 45 == 0;
			double outer = major ? TickOuterR + 2 : TickOuterR;
			double inner = major ? TickInnerR - 2 : TickInnerR + 1;
			double rad = DegToRad(deg);

			CompassDial.Children.Add(new Line
			{
				X1 = Cx + inner * Math.Sin(rad),
				Y1 = Cy - inner * Math.Cos(rad),
				X2 = Cx + outer * Math.Sin(rad),
				Y2 = Cy - outer * Math.Cos(rad),
				Stroke = stroke,
				StrokeThickness = major ? 1.6 : 1,
			}.WithBounds(0, 0, DialSize, DialSize));
		}
	}

	void AddPoint(double deg, double tipR, Color darkFill, Color lightFill, Color stroke)
	{
		double rad = DegToRad(deg);
		double leftRad = DegToRad(deg - HalfSpanDeg);
		double rightRad = DegToRad(deg + HalfSpanDeg);

		double tipX = Cx + tipR * Math.Sin(rad);
		double tipY = Cy - tipR * Math.Cos(rad);

		double leftX = Cx + WaistR * Math.Sin(leftRad);
		double leftY = Cy - WaistR * Math.Cos(leftRad);
		double rightX = Cx + WaistR * Math.Sin(rightRad);
		double rightY = Cy - WaistR * Math.Cos(rightRad);

		CompassDial.Children.Add(MakeTriangle(tipX, tipY, Cx, Cy, leftX, leftY, lightFill, stroke));
		CompassDial.Children.Add(MakeTriangle(tipX, tipY, Cx, Cy, rightX, rightY, darkFill, stroke));
	}

	static Polygon MakeTriangle(double x1, double y1, double x2, double y2, double x3, double y3, Color fill, Color stroke)
	{
		var poly = new Polygon
		{
			Points =
			[
				new Point(x1, y1),
				new Point(x2, y2),
				new Point(x3, y3),
			],
			Fill = fill,
			Stroke = stroke,
			StrokeThickness = 0.5,
		};
		return poly.WithBounds(0, 0, DialSize, DialSize);
	}

	void AddLabel(IDirection dir, Color ink, double thaiSize)
	{
		double rad = DegToRad(dir.Degrees);
		double r = dir.Degrees is 90 or 270 ? LabelREastWest : LabelR;
		double x = Cx + r * Math.Sin(rad);
		double y = Cy - r * Math.Cos(rad);

		bool ordinal = dir.Degrees % 90 != 0;
		double boxW = ordinal ? 130 : 100;
		double boxH = ordinal ? 56 : 36;

		var thaiLabel = new Label
		{
			Text = dir.Spelling,
			FontSize = thaiSize,
			FontAttributes = FontAttributes.Bold,
			TextColor = ink,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center,
			LineBreakMode = LineBreakMode.WordWrap,
			WidthRequest = boxW,
			BackgroundColor = Colors.Transparent,
		};
		thaiLabel.SetDynamicResource(Label.FontFamilyProperty, "CurrentThaiFont");

		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) => ViewModel.SelectDirectionCommand.Execute(dir.Degrees);
		thaiLabel.GestureRecognizers.Add(tap);

		AbsoluteLayout.SetLayoutBounds(thaiLabel, new Rect(x - boxW / 2, y - boxH / 2, boxW, boxH));
		AbsoluteLayout.SetLayoutFlags(thaiLabel, AbsoluteLayoutFlags.None);
		CompassDial.Children.Add(thaiLabel);
		_labelStacks[dir.Degrees] = thaiLabel;
	}

	void ApplySelectionColors()
	{
		bool dark = Application.Current?.RequestedTheme == AppTheme.Dark;
		Color ink = dark ? Color.FromArgb("#C9B8A8") : Color.FromArgb("#5C4033");
		Color accent = dark ? Color.FromArgb("#E8C547") : Color.FromArgb("#B8860B");
		int? selected = ViewModel.SelectedDirection?.Degrees;

		foreach (var (deg, label) in _labelStacks)
		{
			label.BackgroundColor = Colors.Transparent;
			label.TextColor = selected == deg ? accent : ink;
		}
	}

	static double DegToRad(double deg) => deg * Math.PI / 180.0;
}

file static class AbsoluteLayoutExtensions
{
	public static T WithBounds<T>(this T view, double x, double y, double w, double h) where T : View
	{
		AbsoluteLayout.SetLayoutBounds(view, new Rect(x, y, w, h));
		AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.None);
		return view;
	}
}
