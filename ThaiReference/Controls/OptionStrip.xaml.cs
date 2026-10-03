using System.Collections;
using System.Reflection;
using Microsoft.Maui.Controls.Shapes;

namespace ThaiReference.Controls;

public sealed partial class OptionStrip : ContentView
{
    private const string ChromeFontFamilyKey = "CurrentChromeFont";

    private static class Metrics
    {
        public const double TrackCornerRadius = 10;
        public const double SegmentPadH = 8;
        public const double SegmentPadV = 7;
        public const double SeparatorMinLogical = 0.5;
        public const double VerticalSegmentMinHeight = 40;
        public const double MinimumTouch = 36;
    }

    private static readonly Dictionary<(Type Type, string Name), PropertyInfo?> DisplayPropertyCache = [];

    private const string DefaultTextPropertyName = "Thai";

    private readonly Dictionary<object, Border> _borders = [];
    private readonly Dictionary<Border, (int Index, int Count)> _segmentMeta = [];
    private object? _lastPaintedSelection;

    public OptionStrip()
    {
        InitializeComponent();
        ApplyTrackJoin();
    }

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource),
        typeof(IEnumerable),
        typeof(OptionStrip),
        propertyChanged: (b, _, __) => ((OptionStrip)b).OnItemsSourceChanged());

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem),
        typeof(object),
        typeof(OptionStrip),
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (b, _, __) => ((OptionStrip)b).PaintSelectionDelta());

    public static readonly BindableProperty TextPropertyNameProperty = BindableProperty.Create(
        nameof(TextPropertyName),
        typeof(string),
        typeof(OptionStrip),
        DefaultTextPropertyName,
        propertyChanged: (b, _, __) => ((OptionStrip)b).Rebuild());

    public static readonly BindableProperty StripOrientationProperty = BindableProperty.Create(
        nameof(StripOrientation),
        typeof(StackOrientation),
        typeof(OptionStrip),
        StackOrientation.Horizontal,
        propertyChanged: (b, _, __) => ((OptionStrip)b).ApplyOrientation());

    public static readonly BindableProperty AllowNullSelectionProperty = BindableProperty.Create(
        nameof(AllowNullSelection),
        typeof(bool),
        typeof(OptionStrip),
        false,
        propertyChanged: (b, _, __) => ((OptionStrip)b).Rebuild());

    public static readonly BindableProperty JoinProperty = BindableProperty.Create(
        nameof(Join),
        typeof(OptionStripJoin),
        typeof(OptionStrip),
        OptionStripJoin.None,
        propertyChanged: (b, _, __) => ((OptionStrip)b).OnJoinChanged());

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public string TextPropertyName
    {
        get => (string)GetValue(TextPropertyNameProperty);
        set => SetValue(TextPropertyNameProperty, value);
    }

    public StackOrientation StripOrientation
    {
        get => (StackOrientation)GetValue(StripOrientationProperty);
        set => SetValue(StripOrientationProperty, value);
    }

    public bool AllowNullSelection
    {
        get => (bool)GetValue(AllowNullSelectionProperty);
        set => SetValue(AllowNullSelectionProperty, value);
    }

    public OptionStripJoin Join
    {
        get => (OptionStripJoin)GetValue(JoinProperty);
        set => SetValue(JoinProperty, value);
    }

    private void OnItemsSourceChanged() => Rebuild();

	/// <summary>Re-read display text from items (e.g. after UI language change) without resetting selection.</summary>
	public void RefreshCaptions()
	{
		foreach (var (item, border) in _borders)
		{
			if (border.Content is not Label label)
				continue;
			var text = GetDisplayText(item);
			label.Text = text;
			UpdateSegmentAccessibility(border, text, ReferenceEquals(SelectedItem, item));
		}
	}

    private void OnJoinChanged()
    {
        ApplyTrackJoin();
        PaintAllSegments();
    }

    private void ApplyTrackJoin()
    {
        var r = Metrics.TrackCornerRadius;
        var corners = Join switch
        {
            OptionStripJoin.StackStart => new CornerRadius(r, r, 0, 0),
            OptionStripJoin.StackEnd => new CornerRadius(0, 0, r, r),
            _ => new CornerRadius(r),
        };

        if (Track.StrokeShape is RoundRectangle existing)
            existing.CornerRadius = corners;
        else
            Track.StrokeShape = new RoundRectangle { CornerRadius = corners };

        Padding = Join switch
        {
            OptionStripJoin.StackStart => new Thickness(0, 2, 0, 0),
            OptionStripJoin.StackEnd => new Thickness(0, 0, 0, 2),
            _ => new Thickness(0, 2),
        };
    }

    private void ApplyOrientation()
    {
        _borders.Clear();
        _segmentMeta.Clear();
        _lastPaintedSelection = null;
        Rebuild();
    }

    private void Rebuild()
    {
        SegmentGrid.Children.Clear();
        SegmentGrid.ColumnDefinitions.Clear();
        SegmentGrid.RowDefinitions.Clear();
        _borders.Clear();
        _segmentMeta.Clear();
        _lastPaintedSelection = null;

        if (ItemsSource is null)
            return;

        var list = ItemsSource.Cast<object?>().Where(x => x is not null).Cast<object>().ToList();
        var n = list.Count;
        if (n == 0)
            return;

        if (StripOrientation == StackOrientation.Horizontal)
            BuildHorizontalStrip(list, n);
        else
            BuildVerticalStrip(list, n);

        if (AllowNullSelection)
        {
            if (SelectedItem is not null && !list.Contains(SelectedItem))
                SelectedItem = null;
        }
        else
        {
            if (SelectedItem is null || !list.Contains(SelectedItem))
                SelectedItem = list[0];
        }

        PaintAllSegments();
    }

    private void BuildHorizontalStrip(IReadOnlyList<object> list, int n)
    {
        SegmentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        for (var i = 0; i < n; i++)
        {
            SegmentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            if (i < n - 1)
                SegmentGrid.ColumnDefinitions.Add(new ColumnDefinition(SeparatorGridLength()));
        }

        for (var i = 0; i < n; i++)
        {
            var item = list[i];
            var border = CreateSegment(item);
            _borders[item] = border;
            _segmentMeta[border] = (i, n);
            Grid.SetRow(border, 0);
            Grid.SetColumn(border, SegmentGridIndex(i));
            SegmentGrid.Children.Add(border);

            if (i >= n - 1)
                continue;
            var sep = CreateSeparator();
            Grid.SetRow(sep, 0);
            Grid.SetColumn(sep, SegmentGridIndex(i) + 1);
            SegmentGrid.Children.Add(sep);
        }
    }

    private void BuildVerticalStrip(IReadOnlyList<object> list, int n)
    {
        SegmentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        for (var i = 0; i < n; i++)
        {
            SegmentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            if (i < n - 1)
                SegmentGrid.RowDefinitions.Add(new RowDefinition(SeparatorGridLength()));
        }

        for (var i = 0; i < n; i++)
        {
            var item = list[i];
            var border = CreateSegment(item);
            _borders[item] = border;
            _segmentMeta[border] = (i, n);
            Grid.SetColumn(border, 0);
            Grid.SetRow(border, SegmentGridIndex(i));
            SegmentGrid.Children.Add(border);

            if (i >= n - 1)
                continue;
            var sep = CreateSeparator();
            Grid.SetColumn(sep, 0);
            Grid.SetRow(sep, SegmentGridIndex(i) + 1);
            SegmentGrid.Children.Add(sep);
        }
    }

    private static int SegmentGridIndex(int segmentIndex) => segmentIndex * 2;

    private static GridLength SeparatorGridLength()
    {
        var d = DeviceDisplay.MainDisplayInfo.Density;
        if (d <= 0)
            d = 1;
        var logical = 1.0 / d;
        return new GridLength(Math.Max(logical, Metrics.SeparatorMinLogical), GridUnitType.Absolute);
    }

    private BoxView CreateSeparator()
    {
        var box = new BoxView { Style = RequireStyle("OptionStripSeparator") };
        AutomationProperties.SetIsInAccessibleTree(box, false);
        return box;
    }

    private Border CreateSegment(object item)
    {
        var label = new Label
        {
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.TailTruncation,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            Text = GetDisplayText(item),
        };
        label.SetDynamicResource(Label.FontFamilyProperty, ChromeFontFamilyKey);

        var border = new Border
        {
            Content = label,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            if (AllowNullSelection && ReferenceEquals(SelectedItem, item))
                SelectedItem = null;
            else if (!ReferenceEquals(SelectedItem, item))
                SelectedItem = item;
        };
        border.GestureRecognizers.Add(tap);

        AutomationProperties.SetIsInAccessibleTree(border, true);
        AutomationProperties.SetIsInAccessibleTree(label, false);

        border.MinimumHeightRequest = StripOrientation == StackOrientation.Vertical
            ? Math.Max(Metrics.VerticalSegmentMinHeight, Metrics.MinimumTouch)
            : Metrics.MinimumTouch;

        border.Style = RequireStyle("OptionStripSegmentBorderIdle");
        label.Style = RequireStyle("OptionStripCaptionIdle");

        UpdateSegmentAccessibility(border, GetDisplayText(item), ReferenceEquals(SelectedItem, item));

        return border;
    }

    private void PaintSelectionDelta()
    {
        var cur = SelectedItem;
        if (ReferenceEquals(_lastPaintedSelection, cur))
            return;

        if (_lastPaintedSelection != null && _borders.TryGetValue(_lastPaintedSelection, out var prev))
            PaintSegment(prev, selected: false);

        if (cur != null && _borders.TryGetValue(cur, out var next))
            PaintSegment(next, selected: true);

        _lastPaintedSelection = cur;
    }

    private void PaintAllSegments()
    {
        _lastPaintedSelection = null;
        foreach (var kv in _borders)
            PaintSegment(kv.Value, ReferenceEquals(SelectedItem, kv.Key));
        _lastPaintedSelection = SelectedItem;
    }

    private void PaintSegment(Border border, bool selected)
    {
        if (border.Content is not Label label)
            return;

        if (selected)
        {
            border.Style = RequireStyle("OptionStripSegmentBorderSelected");
            label.Style = RequireStyle("OptionStripCaptionSelected");
            if (border.StrokeShape is RoundRectangle selShape)
            {
                var (index, count) = _segmentMeta.TryGetValue(border, out var m) ? m : (0, 1);
                var horizontal = StripOrientation == StackOrientation.Horizontal;
                selShape.CornerRadius = SelectedHighlightCorners(index, count, horizontal);
            }

            border.ClearValue(Border.ShadowProperty);
            UpdateSegmentAccessibility(border, label.Text ?? "", selected: true);
        }
        else
        {
            border.Style = RequireStyle("OptionStripSegmentBorderIdle");
            label.Style = RequireStyle("OptionStripCaptionIdle");
            if (border.StrokeShape is RoundRectangle unselShape)
                unselShape.CornerRadius = 0;
            border.ClearValue(Border.ShadowProperty);
            UpdateSegmentAccessibility(border, label.Text ?? "", selected: false);
        }
    }

    private Style RequireStyle(string key)
    {
        if (!Resources.TryGetValue(key, out var o) || o is not Style s)
            throw new InvalidOperationException($"Missing or invalid Style '{key}' for OptionStrip.");
        return s;
    }

    private void UpdateSegmentAccessibility(Border border, string displayText, bool selected)
    {
        AutomationProperties.SetName(border, displayText);
        AutomationProperties.SetHelpText(border, selected
            ? (AllowNullSelection ? "Selected. Tap again to show all." : "Currently selected.")
            : "Tap to select.");
        SemanticProperties.SetDescription(border, selected ? $"{displayText}, selected." : $"{displayText}. Tap to select.");
        if (selected)
            border.ClearValue(SemanticProperties.HintProperty);
        else
            SemanticProperties.SetHint(border, AllowNullSelection ? "Applies this filter. Tap again to clear." : "Selects this option.");
    }

    /// <summary>
    /// Outer corners follow the track shell (including Join); edges against separators stay square.
    /// Order: top-left, top-right, bottom-left, bottom-right.
    /// </summary>
    private CornerRadius SelectedHighlightCorners(int index, int count, bool horizontal)
    {
        var outerR = Metrics.TrackCornerRadius;
        var roundTop = Join is OptionStripJoin.None or OptionStripJoin.StackStart;
        var roundBottom = Join is OptionStripJoin.None or OptionStripJoin.StackEnd;
        var roundLeft = Join is OptionStripJoin.None or OptionStripJoin.StackStart;
        var roundRight = Join is OptionStripJoin.None or OptionStripJoin.StackEnd;

        if (count <= 1)
        {
            if (horizontal)
            {
                return new CornerRadius(
                    roundTop ? outerR : 0,
                    roundTop ? outerR : 0,
                    roundBottom ? outerR : 0,
                    roundBottom ? outerR : 0);
            }

            return new CornerRadius(
                roundLeft ? outerR : 0,
                roundRight ? outerR : 0,
                roundLeft ? outerR : 0,
                roundRight ? outerR : 0);
        }

        if (horizontal)
        {
            if (index == 0)
                return new CornerRadius(roundTop ? outerR : 0, 0, roundBottom ? outerR : 0, 0);
            if (index == count - 1)
                return new CornerRadius(0, roundTop ? outerR : 0, 0, roundBottom ? outerR : 0);
            return default;
        }

        if (index == 0)
            return new CornerRadius(roundLeft ? outerR : 0, roundRight ? outerR : 0, 0, 0);
        if (index == count - 1)
            return new CornerRadius(0, 0, roundLeft ? outerR : 0, roundRight ? outerR : 0);
        return default;
    }

    private string GetDisplayText(object item)
    {
        var name = TextPropertyName;
        if (string.IsNullOrEmpty(name))
            return item.ToString() ?? "";

        var key = (item.GetType(), name);
        if (!DisplayPropertyCache.TryGetValue(key, out var prop))
        {
            prop = item.GetType().GetRuntimeProperty(name);
            DisplayPropertyCache[key] = prop;
        }

        if (prop?.GetValue(item) is { } v)
            return v.ToString() ?? "";
        return item.ToString() ?? "";
    }
}
