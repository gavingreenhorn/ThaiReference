using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Audio;
using ThaiReference.Contracts;
using ThaiReference.DataAccess;
using ThaiReference.Messages;
using ThaiReference.Models;
using ThaiReference.Models.Interfaces;
using ThaiReference.Ui;
using ThaiReference.Utilities;
using ThaiReference.Utilities.Extensions;

namespace ThaiReference.ViewModels;

public readonly record struct ClusterMatrixRow(ConsonantItem RowHeader, ClusterMatrixCellSlot[] Cells);

/// <summary>One matrix column slot; always present so layout keeps equal column widths when a cell is empty.</summary>
public sealed class ClusterMatrixCellSlot(ClusterRowVm? cluster)
{
	public ClusterRowVm? Cluster { get; } = cluster;
	public bool HasCluster => Cluster is not null;
	public string ThaiCluster => Cluster?.ThaiCluster ?? string.Empty;
}

public partial class ClusterRowVm : ObservableObject
{
    [ObservableProperty]
    string thaiCluster = "";

    [ObservableProperty]
    string? subtype;

    [ObservableProperty]
    ObservableCollection<IWord> sampleWords = [];

    public ClusterRowVm(string thaiCluster, string? subtype, IEnumerable<IWord> words)
    {
        ThaiCluster = thaiCluster;
        Subtype = subtype;
        SampleWords = new ObservableCollection<IWord>(words);
    }
}

public partial class ClusterSectionVm : ObservableObject
{
	public string? TitleId { get; }
	public bool ShowSeparatorAbove { get; }
	public ClusterRowVm[] Clusters { get; }

	[ObservableProperty]
	string? title;

	public ClusterSectionVm(string? titleId, ClusterRowVm[] clusters, bool showSeparatorAbove = false)
	{
		TitleId = titleId;
		Clusters = clusters;
		ShowSeparatorAbove = showSeparatorAbove;
	}

	public void ApplyLanguage(UiLanguage language) =>
		Title = TitleId is null ? null : UiLabels.Get(TitleId, language);
}

public partial class ClusterTypePaneVm : ObservableObject
{
	public string TypeSlug { get; }
	public string TitleId { get; }
	public ClusterAnatomyType? AnatomyType { get; }
	public ClusterMatrixRow[] MatrixRows { get; }
	public ConsonantItem[]? MatrixColumnHeaders { get; }
	public ClusterRowVm[] Clusters { get; }
	public ClusterSectionVm[] ClusterSections { get; }

	[ObservableProperty]
	string title = "";

	[ObservableProperty]
	ClusterRowVm? selectedCluster;

	public bool HasSelectedCluster => SelectedCluster != null;

	public RelayCommand<ClusterRowVm?> SelectClusterCommand { get; }

	public RelayCommand ClearSelectionCommand { get; }

	public ClusterTypePaneVm(
		string typeSlug,
		string titleId,
		ClusterAnatomyType? anatomyType,
		ClusterMatrixRow[] matrixRows,
		ConsonantItem[]? matrixColumnHeaders,
		ClusterRowVm[] clusters,
		ClusterSectionVm[]? clusterSections = null)
	{
		TypeSlug = typeSlug;
		TitleId = titleId;
		AnatomyType = anatomyType;
		MatrixRows = matrixRows;
		MatrixColumnHeaders = matrixColumnHeaders;
		Clusters = clusters;
		ClusterSections = clusterSections ?? [];
		SelectClusterCommand = new RelayCommand<ClusterRowVm?>(r =>
		{
			if (r == null) return;
			if (ReferenceEquals(SelectedCluster, r))
				SelectedCluster = null;
			else
				SelectedCluster = r;
		});
		ClearSelectionCommand = new RelayCommand(() => SelectedCluster = null);
	}

	public void ApplyLanguage(UiLanguage language)
	{
		Title = UiLabels.Get(TitleId, language);
		foreach (var section in ClusterSections)
			section.ApplyLanguage(language);
	}

	partial void OnSelectedClusterChanged(ClusterRowVm? value) => OnPropertyChanged(nameof(HasSelectedCluster));
}

public partial class ConsonantClustersViewModel : ObservableObject, IRomanizationReceiver, IVoiced, IDetailsNavigable
{
    private static readonly char[] TrueRowConsonants = ['ก', 'ข', 'ค', 'ต', 'ป', 'ผ', 'พ'];
    private static readonly char[] TrueColConsonants = ['ร', 'ล', 'ว'];
    private static readonly char[] ToneShiftColConsonants = ['ห', 'อ'];
    private static readonly string[] CardTypeOrder =
        ["true", "false", "leading", "enepenthetic", "non-sonorant", "sub-syllable"];

    private static readonly string[] EnepentheticSubtypeOrder = ["enepenthetic", "enepenthetic_preposed"];

    private static readonly string[] NonSonorantSubtypeOrder =
        ["low_class_second", "mid_class_second", "high_class_second"];

	private static readonly Dictionary<string, ClusterAnatomyType> CardAnatomy =
		new(StringComparer.Ordinal)
		{
			["true"] = ClusterAnatomyType.True,
			["false"] = ClusterAnatomyType.False,
			["leading"] = ClusterAnatomyType.Leading,
			["enepenthetic"] = ClusterAnatomyType.Enepenthetic,
			["non-sonorant"] = ClusterAnatomyType.NonSonorant,
			["sub-syllable"] = ClusterAnatomyType.SubSyllable,
		};

    public IAudioManager AudioManager { get; init; }
    public RelayCommand<object> PronounceCommand { get; init; }
    public RelayCommand<IHaveDescription> ToDetailsCommand { get; init; }
    public RelayCommand<ClusterRowVm?> SelectClusterCommand { get; init; }

    [ObservableProperty]
    string romanizationOption = "IPA";

    [ObservableProperty]
    ObservableCollection<ClusterTypePaneVm> typePanes = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveClusterAnatomyType), nameof(HasVisibleSamplesStrip))]
    ClusterTypePaneVm? activeClusterPane;

    public ClusterAnatomyType ActiveClusterAnatomyType => ActiveClusterPane?.AnatomyType ?? ClusterAnatomyType.True;

    public void ActivatePane(ClusterTypePaneVm pane)
    {
        if (ReferenceEquals(ActiveClusterPane, pane))
            return;

        if (ActiveClusterPane?.HasSelectedCluster == true)
            ActiveClusterPane.ClearSelectionCommand.Execute(null);

        ActiveClusterPane = pane;
    }

	public ConsonantClustersViewModel(IDataServiceFactory dataServiceFactory, IAudioManager audioManager)
	{
		AudioManager = audioManager;
		WeakReferenceMessenger.Default.Register(this);
		WeakReferenceMessenger.Default.Register<UiLanguageChanged>(this, (_, m) => ApplyLanguage(m.Value));
		PronounceCommand = new RelayCommand<object>(async o => await this.Pronounce(o));
		ToDetailsCommand = new RelayCommand<IHaveDescription>(async item => await this.ToDetailsAsync(item));
		SelectClusterCommand = new RelayCommand<ClusterRowVm?>(row => ActiveClusterPane?.SelectClusterCommand.Execute(row));
		TypePanes = BuildTypePanes(dataServiceFactory.Get("thai"));
		ActiveClusterPane = TypePanes.FirstOrDefault();
		ApplyLanguage(UiLanguageHelper.Read());
	}

	public void Receive(RomanizationOptionChanged message) => RomanizationOption = message.Value;

	void ApplyLanguage(UiLanguage language)
	{
		foreach (var pane in TypePanes)
			pane.ApplyLanguage(language);
	}

	private static ObservableCollection<ClusterTypePaneVm> BuildTypePanes(IDataService ds)
	{
		var rowsByType = ds.Clusters
			.GroupBy(c => c.Type, StringComparer.Ordinal)
			.ToDictionary(
				g => g.Key,
				g => g.Select(c => new ClusterRowVm(c.Value, c.Subtype, c.SampleWords)).ToList(),
				StringComparer.Ordinal);

		var consonants = ds.Consonants.ToDictionary(
			c => c.Grapheme.Trim(),
			c => new ConsonantItem(c),
			StringComparer.Ordinal);

		var panes = new List<ClusterTypePaneVm>();
		foreach (var cardType in CardTypeOrder)
		{
			if (!rowsByType.TryGetValue(cardType, out var rows) || rows.Count == 0)
				continue;

			if (!CardAnatomy.TryGetValue(cardType, out var anatomy))
				continue;
			if (!UiLabels.ClusterTypeToId.TryGetValue(cardType, out var titleId))
				continue;

			panes.Add(BuildCardPane(cardType, titleId, anatomy, rows, consonants));
		}

		return new ObservableCollection<ClusterTypePaneVm>(panes);
	}

	private static ClusterTypePaneVm BuildCardPane(
		string cardType,
		string titleId,
		ClusterAnatomyType? anatomy,
		List<ClusterRowVm> rows,
		IReadOnlyDictionary<string, ConsonantItem> consonants) =>
		cardType switch
		{
			"true" => BuildMatrixPane(cardType, titleId, anatomy, BuildTrueClusterMatrix(rows, consonants)),
			"false" => BuildMatrixPane(cardType, titleId, anatomy, BuildFalseClusterMatrix(rows, consonants)),
			"leading" => BuildMatrixPane(cardType, titleId, anatomy, BuildToneShiftMatrix(rows, consonants)),
			"enepenthetic" => BuildSectionedPane(
				cardType, titleId, anatomy, rows,
				EnepentheticSubtypeOrder,
				showSectionHeaders: false),
			"non-sonorant" => BuildSectionedPane(
				cardType, titleId, anatomy, rows,
				NonSonorantSubtypeOrder,
				showSectionHeaders: true),
			"sub-syllable" => BuildSimplePane(cardType, titleId, anatomy, rows),
			_ => throw new InvalidOperationException($"Unhandled cluster card type: {cardType}"),
		};

	private static ClusterTypePaneVm BuildMatrixPane(
		string cardType,
		string titleId,
		ClusterAnatomyType? anatomy,
		(ClusterMatrixRow[] Rows, ConsonantItem[]? ColumnHeaders) matrix) =>
		new(cardType, titleId, anatomy, matrix.Rows, matrix.ColumnHeaders, []);

	private static ClusterTypePaneVm BuildSectionedPane(
		string cardType,
		string titleId,
		ClusterAnatomyType? anatomy,
		List<ClusterRowVm> rows,
		string[] subtypeOrder,
		bool showSectionHeaders)
	{
		var bySubtype = rows
			.GroupBy(r => r.Subtype ?? string.Empty)
			.ToDictionary(g => g.Key, g => g.OrderBy(x => x.ThaiCluster, StringComparer.Ordinal).ToArray());

		var orderedKeys = subtypeOrder
			.Where(bySubtype.ContainsKey)
			.Concat(bySubtype.Keys.Except(subtypeOrder).OrderBy(k => k, StringComparer.Ordinal))
			.ToArray();

		var sections = new List<ClusterSectionVm>();
		for (var i = 0; i < orderedKeys.Length; i++)
		{
			var key = orderedKeys[i];
			string? sectionTitleId = null;
			if (showSectionHeaders)
			{
				if (!UiLabels.ClusterSubtypeToId.TryGetValue(key, out sectionTitleId))
					sectionTitleId = key;
			}
			sections.Add(new ClusterSectionVm(
				sectionTitleId,
				bySubtype[key],
				showSeparatorAbove: showSectionHeaders && i > 0));
		}

		return new(cardType, titleId, anatomy, [], null, [], sections.ToArray());
	}

	private static ClusterTypePaneVm BuildSimplePane(
		string cardType,
		string titleId,
		ClusterAnatomyType? anatomy,
		List<ClusterRowVm> rows) =>
		new(
			cardType,
			titleId,
			anatomy,
			[],
			null,
			rows.OrderBy(r => r.ThaiCluster, StringComparer.Ordinal).ToArray(),
			[]);

	private static (ClusterMatrixRow[] Rows, ConsonantItem[] ColumnHeaders) BuildTrueClusterMatrix(
		IReadOnlyList<ClusterRowVm> rows,
		IReadOnlyDictionary<string, ConsonantItem> consonants)
	{
		var map = new Dictionary<(char r, char c), ClusterRowVm>();
		foreach (var row in rows)
		{
			var v = row.ThaiCluster.Trim();
			if (v.Length < 2)
				continue;
			map[(v[0], v[1])] = row;
		}

		var matrixRows = new ClusterMatrixRow[TrueRowConsonants.Length];
		for (var i = 0; i < TrueRowConsonants.Length; i++)
		{
			var rc = TrueRowConsonants[i];
			var cells = new ClusterMatrixCellSlot[TrueColConsonants.Length];
			for (var j = 0; j < TrueColConsonants.Length; j++)
			{
				map.TryGetValue((rc, TrueColConsonants[j]), out var cluster);
				cells[j] = new ClusterMatrixCellSlot(cluster);
			}

			matrixRows[i] = new(consonants[rc.ToString()], cells);
		}

		var columnHeaders = TrueColConsonants.Select(cc => consonants[cc.ToString()]).ToArray();
		return (matrixRows, columnHeaders);
	}

	private static (ClusterMatrixRow[] Rows, ConsonantItem[]? ColumnHeaders) BuildFalseClusterMatrix(
		IReadOnlyList<ClusterRowVm> rows,
		IReadOnlyDictionary<string, ConsonantItem> consonants)
	{
		var map = new Dictionary<(char First, char Second), ClusterRowVm>();
		foreach (var row in rows)
		{
			var v = row.ThaiCluster.Trim();
			if (v.Length < 2)
				continue;
			map[(v[0], v[1])] = row;
		}

		if (map.Count == 0)
			return ([], null);

		var rowChars = map.Keys.Select(k => k.First).Distinct().OrderBy(c => c.ToString(), StringComparer.Ordinal).ToArray();
		var colChars = map.Keys.Select(k => k.Second).Distinct().OrderBy(c => c.ToString(), StringComparer.Ordinal).ToArray();

		var matrixRows = new ClusterMatrixRow[rowChars.Length];
		for (var i = 0; i < rowChars.Length; i++)
		{
			var rc = rowChars[i];
			var cells = new ClusterMatrixCellSlot[colChars.Length];
			for (var j = 0; j < colChars.Length; j++)
			{
				map.TryGetValue((rc, colChars[j]), out var cluster);
				cells[j] = new ClusterMatrixCellSlot(cluster);
			}

			matrixRows[i] = new(consonants[rc.ToString()], cells);
		}

		var columnHeaders = colChars.Select(cc => consonants[cc.ToString()]).ToArray();
		return (matrixRows, columnHeaders);
	}

	private static (ClusterMatrixRow[] Rows, ConsonantItem[] ColumnHeaders) BuildToneShiftMatrix(
		IReadOnlyList<ClusterRowVm> rows,
		IReadOnlyDictionary<string, ConsonantItem> consonants)
	{
		var map = new Dictionary<(char Leader, char Second), ClusterRowVm>();
		foreach (var row in rows)
		{
			var v = row.ThaiCluster.Trim();
			if (v.Length < 2)
				continue;
			map[(v[0], v[1])] = row;
		}

		var rowChars = map.Keys
			.Select(k => k.Second)
			.Distinct()
			.OrderBy(c => c.ToString(), StringComparer.Ordinal)
			.ToArray();

		var matrixRows = new ClusterMatrixRow[rowChars.Length];
		for (var i = 0; i < rowChars.Length; i++)
		{
			var second = rowChars[i];
			var cells = new ClusterMatrixCellSlot[ToneShiftColConsonants.Length];
			for (var j = 0; j < ToneShiftColConsonants.Length; j++)
			{
				map.TryGetValue((ToneShiftColConsonants[j], second), out var cluster);
				cells[j] = new ClusterMatrixCellSlot(cluster);
			}

			matrixRows[i] = new(consonants[second.ToString()], cells);
		}

		var columnHeaders = ToneShiftColConsonants.Select(cc => consonants[cc.ToString()]).ToArray();
		return (matrixRows, columnHeaders);
	}

    private ClusterTypePaneVm? _selectionSubscriptionPane;

    partial void OnActiveClusterPaneChanged(ClusterTypePaneVm? value)
    {
        if (_selectionSubscriptionPane != null)
            _selectionSubscriptionPane.PropertyChanged -= OnActivePaneChildPropertyChanged;
        _selectionSubscriptionPane = value;
        if (_selectionSubscriptionPane != null)
            _selectionSubscriptionPane.PropertyChanged += OnActivePaneChildPropertyChanged;
    }

    private void OnActivePaneChildPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ClusterTypePaneVm.SelectedCluster) or nameof(ClusterTypePaneVm.HasSelectedCluster))
            OnPropertyChanged(nameof(HasVisibleSamplesStrip));
    }

	public bool HasVisibleSamplesStrip => ActiveClusterPane?.HasSelectedCluster == true;
}
