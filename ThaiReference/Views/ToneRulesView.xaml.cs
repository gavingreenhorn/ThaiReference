using System.ComponentModel;
using ThaiReference.Ui;
using ThaiReference.Utilities;
using ThaiReference.ViewModels;

namespace ThaiReference.Views;

public partial class ToneRulesView : ContentPage
{
    /// <summary>Col 0 = class labels; cols 1–7 = Live, Dead long, Dead short, ่, ้, ๊, ๋.</summary>
    const int ColCount = 8;

    const double HeaderRowH = 34;
    const double SubHeaderRowH = 30;
    const double DataRowH = 52;

    readonly ToneRulesViewModel _vm;
    bool _built;
    double _lastWidth = -1;

    Color _ink = Colors.White;
    Color _border = Colors.Gray;
    Color _headerBg = Colors.DimGray;
    Color _emptyBg = Colors.Gray;
    Color _cellBg = Colors.Black;
    Color _selectedBg = Colors.DarkSlateGray;
    Color _selectedStroke = Colors.Goldenrod;

    public ToneRulesView(ToneRulesViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        _vm.PropertyChanged += OnVmPropertyChanged;
    }

	void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(ToneRulesViewModel.SelectedCell))
			HighlightSelection();
		else if (e.PropertyName == nameof(ToneRulesViewModel.ClassRowLabels) && _built && ChartHost.Width > 0)
			BuildChart();
	}

    void OnChartHostSizeChanged(object? sender, EventArgs e)
    {
        // Full width only — do not size rows from host height.
        if (ChartHost.Width <= 0 || Math.Abs(ChartHost.Width - _lastWidth) < 0.5)
            return;
        _lastWidth = ChartHost.Width;
        BuildChart();
    }

    void BuildChart()
    {
        ChartHost.Children.Clear();
        ChartHost.RowDefinitions.Clear();
        ChartHost.ColumnDefinitions.Clear();

        bool dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        _ink = ThemeColors.Get(dark ? "AccentDark" : "AccentLight");
        _border = ThemeColors.Get(dark ? "PanelStrokeDark" : "PanelStrokeLight");
        _headerBg = ThemeColors.Get(dark ? "Gray600" : "Gray200");
        _emptyBg = ThemeColors.Get(dark ? "Gray900" : "Gray300");
        _cellBg = ThemeColors.Get(dark ? "SurfaceInsetDark" : "SurfaceInsetLight");
        _selectedBg = ThemeColors.Get("ControlBG3");
        _selectedStroke = ThemeColors.Get(dark ? "AccentDark" : "ControlFG2");

        ChartHost.RowDefinitions.Add(new RowDefinition { Height = HeaderRowH });
        ChartHost.RowDefinitions.Add(new RowDefinition { Height = SubHeaderRowH });
        ChartHost.RowDefinitions.Add(new RowDefinition { Height = SubHeaderRowH });
        for (int i = 0; i < 3; i++)
            ChartHost.RowDefinitions.Add(new RowDefinition { Height = DataRowH });

        ChartHost.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.15, GridUnitType.Star) });
        for (int c = 1; c < ColCount; c++)
            ChartHost.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

		var lang = UiLanguageState.Instance.Language;
		string L(string cellId) => UiLabels.Get(UiLabels.ToneRules.TitleId(cellId), lang);

		// Row 0 headers
		AddCell(L("hdr.class"), 0, 0, 1, 3, _headerBg, "hdr.class", fontSize: 11, bold: true);
		AddCell(L("hdr.syllable"), 0, 1, 3, 1, _headerBg, "hdr.syllable", fontSize: 13, bold: true);
		AddCell(L("hdr.tonemark"), 0, 4, 4, 1, _headerBg, "hdr.tonemark", fontSize: 13, bold: true);

		// Row 1–2: Live / Dead(+Long/Short) / tone marks
		AddCell(L("hdr.live"), 1, 1, 1, 2, _headerBg, "hdr.live", fontSize: 14, bold: true);
		AddCell(L("hdr.dead"), 1, 2, 2, 1, _headerBg, "hdr.dead", fontSize: 14, bold: true);
		AddCell(UiLabels.Get(UiLabels.ToneRules.ChartLong, lang), 2, 2, 1, 1, _headerBg, "hdr.long", fontSize: 12, bold: true);
		AddCell(UiLabels.Get(UiLabels.ToneRules.ChartShort, lang), 2, 3, 1, 1, _headerBg, "hdr.short", fontSize: 12, bold: true);
		AddCell("◌่", 1, 4, 1, 2, _headerBg, "hdr.mai_ek", fontSize: 22, bold: true);
		AddCell("◌้", 1, 5, 1, 2, _headerBg, "hdr.mai_tho", fontSize: 22, bold: true);
		AddCell("◌๊", 1, 6, 1, 2, _headerBg, "hdr.mai_tri", fontSize: 22, bold: true);
		AddCell("◌๋", 1, 7, 1, 2, _headerBg, "hdr.mai_chattawa", fontSize: 22, bold: true);

        // Data rows (catalog cols 0–6 map to grid cols 1–7)
        string[] classIds = ["hdr.low", "hdr.mid", "hdr.high"];
        for (int r = 0; r < 3; r++)
        {
            int gridRow = r + 3;
			string classLabel = r < _vm.ClassRowLabels.Count
				? _vm.ClassRowLabels[r]
				: UiLabels.Get(UiLabels.ToneRules.TitleId(classIds[r]), lang);
			AddCell(classLabel, gridRow, 0, 1, 1, _headerBg, classIds[r], fontSize: 16, bold: true);

            bool mergeDead = r is 1 or 2;
            for (int c = 0; c < 7; c++)
            {
                int gridCol = c + 1;
                var (glyph, cellId) = ToneRulesViewModel.Cells[r, c];

                if (mergeDead && c == 2)
                    continue; // covered by span from dead long

                if (glyph is null || cellId is null)
                {
                    AddCell("", gridRow, gridCol, 1, 1, _emptyBg, null, tappable: false);
                    continue;
                }

                if (mergeDead && c == 1)
                {
                    AddCell(glyph, gridRow, gridCol, 2, 1, _cellBg, cellId, fontSize: 22, bold: true);
                    continue;
                }

                AddCell(glyph, gridRow, gridCol, 1, 1, _cellBg, cellId, fontSize: 22, bold: true);
            }
        }

        _built = true;
        HighlightSelection();
    }

    void AddCell(
        string text,
        int row,
        int col,
        int colSpan,
        int rowSpan,
        Color bg,
        string? cellId,
        double fontSize = 14,
        bool bold = false,
        bool tappable = true)
    {
        var label = new Label
        {
            Text = text,
            FontSize = fontSize,
            FontAttributes = bold ? FontAttributes.Bold : FontAttributes.None,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            TextColor = _ink,
            LineBreakMode = LineBreakMode.WordWrap,
        };

        // Vertical-ish label for the class axis header
        if (cellId == "hdr.class")
        {
            label.FontSize = 11;
            label.LineBreakMode = LineBreakMode.CharacterWrap;
        }

        var cell = new Border
        {
            Stroke = _border,
            StrokeThickness = 1,
            BackgroundColor = bg,
            Padding = new Thickness(4),
            Content = label,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
        };

        if (tappable && cellId is not null)
        {
            cell.StyleId = $"tone-{cellId}";
            var tap = new TapGestureRecognizer();
            var id = cellId;
            tap.Tapped += (_, _) =>
            {
                var model = _vm.GetCell(id);
                if (model is not null)
                    _vm.SelectCommand.Execute(model);
            };
            cell.GestureRecognizers.Add(tap);
        }

        Grid.SetRow(cell, row);
        Grid.SetColumn(cell, col);
        if (colSpan > 1)
            Grid.SetColumnSpan(cell, colSpan);
        if (rowSpan > 1)
            Grid.SetRowSpan(cell, rowSpan);
        ChartHost.Children.Add(cell);
    }

    void HighlightSelection()
    {
        if (!_built) return;
        string? id = _vm.SelectedCell?.Id;
        foreach (var child in ChartHost.Children)
        {
            if (child is not Border b || string.IsNullOrEmpty(b.StyleId))
                continue;
            if (!b.StyleId.StartsWith("tone-", StringComparison.Ordinal))
                continue;
            if (b.BackgroundColor == _emptyBg)
                continue;

            bool selected = id is not null && b.StyleId == $"tone-{id}";
            bool isHeader = b.StyleId.StartsWith("tone-hdr.", StringComparison.Ordinal);
            b.BackgroundColor = selected ? _selectedBg : (isHeader ? _headerBg : _cellBg);
            b.StrokeThickness = selected ? 2 : 1;
            b.Stroke = selected ? _selectedStroke : _border;
        }
    }
}
