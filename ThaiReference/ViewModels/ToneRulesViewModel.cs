using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ThaiReference.Contracts;
using ThaiReference.DataAccess;
using ThaiReference.Messages;
using ThaiReference.Models.BaseModels;
using ThaiReference.Models.Interfaces;
using ThaiReference.Models.ToneRules;
using ThaiReference.Ui;
using ThaiReference.Utilities;

namespace ThaiReference.ViewModels;

public partial class ToneRulesViewModel : ObservableObject, IRomanizationReceiver
{
	static readonly string[] ClassCodes = ["low", "mid", "high"];

	/// <summary>
	/// Data columns: Live, Dead long, Dead short, ่, ้, ๊, ๋.
	/// Mid/High dead long+short share one cell in the UI (same tone).
	/// </summary>
	public static readonly (string? Glyph, string? CellId)[,] Cells =
	{
		// Low
		{ ("M", "low.live"), ("F", "low.dead.long"), ("H", "low.dead.short"), ("F", "low.mai_ek"), ("H", "low.mai_tho"), (null, null), (null, null) },
		// Mid — dead long/short both L (merged in UI via CellId mid.dead)
		{ ("M", "mid.live"), ("L", "mid.dead"), ("L", "mid.dead"), ("L", "mid.mai_ek"), ("F", "mid.mai_tho"), ("H", "mid.mai_tri"), ("R", "mid.mai_chattawa") },
		// High
		{ ("R", "high.live"), ("L", "high.dead"), ("L", "high.dead"), ("L", "high.mai_ek"), ("F", "high.mai_tho"), (null, null), (null, null) },
	};

	static readonly Dictionary<string, ToneRuleCell> CellsById = BuildCells();

	readonly Dictionary<string, IConsonantClass> _classesByCode;
	readonly Dictionary<string, IMark> _toneMarksByGrapheme;

	[ObservableProperty] private ToneRuleCell? selectedCell;
	[ObservableProperty] private string romanizationOption = "IPA";

	/// <summary>Class or tone-mark name selected for the detail strip (null = rule-text mode).</summary>
	[ObservableProperty] private object? selectedNamed;

	[ObservableProperty] private IReadOnlyList<string> classRowLabels = [];

	public bool HasNamedDetail => SelectedNamed is not null;

	public string DetailThai => SelectedNamed switch
	{
		IConsonantClass c => c.Name,
		IMark m => string.Concat(m.Words.Select(w => w.Value)),
		_ => "",
	};

	public ITranscription? DetailTranscription => SelectedNamed switch
	{
		IConsonantClass c => MergeTranscription(c.Words.ToList()),
		IMark m => MergeTranscription(m.Words.ToList()),
		_ => null,
	};

	public string DetailEnglish =>
		UiLanguageState.Instance.Language != UiLanguage.English
			? ""
			: SelectedNamed switch
			{
				IConsonantClass c => c.EnglishLabel ?? c.Code,
				IMark m => m.EnglishLabel ?? m.Name,
				_ => "",
			};

	public string DetailTitle
	{
		get
		{
			var lang = UiLanguageState.Instance.Language;
			if (HasNamedDetail)
				return DetailEnglish;
			if (SelectedCell is not null)
				return UiLabels.Get(SelectedCell.TitleId, lang);
			return UiLabels.Get(UiLabels.Shell.ToneRules, lang);
		}
	}

	public string DetailBody
	{
		get
		{
			if (HasNamedDetail)
				return "";
			var lang = UiLanguageState.Instance.Language;
			if (SelectedCell is not null)
				return UiLabels.Get(SelectedCell.DetailId, lang);
			return UiLabels.Get(UiLabels.ToneRules.EmptyBody, lang);
		}
	}

	public string DetailGlyph => HasNamedDetail
		? SelectedNamed switch
		{
			IMark m => m.Grapheme,
			_ => "",
		}
		: SelectedCell?.Glyph ?? "";

	public string DetailToneName
	{
		get
		{
			if (HasNamedDetail || SelectedCell is null || string.IsNullOrEmpty(SelectedCell.ToneKey))
				return "";
			var id = ToneKeyToId(SelectedCell.ToneKey);
			return id is null ? "" : UiLabels.Get(id, UiLanguageState.Instance.Language);
		}
	}

	public ToneRulesViewModel(IDataServiceFactory dsf)
	{
		WeakReferenceMessenger.Default.Register(this);
		WeakReferenceMessenger.Default.Register<UiLanguageChanged>(this, (_, m) => ApplyLanguage(m.Value));

		IDataService ds = dsf.Get("thai");
		_classesByCode = ds.Classes
			.GroupBy(c => c.Code, StringComparer.OrdinalIgnoreCase)
			.ToDictionary(g => g.Key.ToLowerInvariant(), g => g.First(), StringComparer.OrdinalIgnoreCase);

		_toneMarksByGrapheme = ds.Marks
			.Where(m => m.ToneMark && m.Words.Any())
			.GroupBy(m => m.Grapheme)
			.ToDictionary(g => g.Key, g => g.First());

		ApplyLanguage(UiLanguageHelper.Read());
	}

	public ToneRuleCell? GetCell(string id) =>
		CellsById.TryGetValue(id, out var cell) ? cell : null;

	[RelayCommand]
	private void Select(ToneRuleCell? cell)
	{
		if (cell is null) return;
		SelectedCell = cell;
		SelectedNamed = ResolveNamed(cell.Id);
	}

	public void Receive(RomanizationOptionChanged message)
	{
		RomanizationOption = message.Value;
	}

	partial void OnSelectedCellChanged(ToneRuleCell? value) => NotifyDetailProps();

	partial void OnSelectedNamedChanged(object? value) => NotifyDetailProps();

	void ApplyLanguage(UiLanguage language)
	{
		ClassRowLabels = ClassCodes
			.Select(code =>
			{
				if (!_classesByCode.TryGetValue(code, out var c))
					return UiLabels.Get(ClassCodeToOptionId(code), language);
				return language == UiLanguage.Thai
					? c.Name
					: (c.EnglishLabel ?? Capitalize(code));
			})
			.ToList();
		NotifyDetailProps();
	}

	void NotifyDetailProps()
	{
		OnPropertyChanged(nameof(HasNamedDetail));
		OnPropertyChanged(nameof(DetailThai));
		OnPropertyChanged(nameof(DetailTranscription));
		OnPropertyChanged(nameof(DetailEnglish));
		OnPropertyChanged(nameof(DetailTitle));
		OnPropertyChanged(nameof(DetailBody));
		OnPropertyChanged(nameof(DetailGlyph));
		OnPropertyChanged(nameof(DetailToneName));
	}

	object? ResolveNamed(string cellId) => cellId switch
	{
		"hdr.low" => LookupClass("low"),
		"hdr.mid" => LookupClass("mid"),
		"hdr.high" => LookupClass("high"),
		"hdr.mai_ek" => LookupMark("่"),
		"hdr.mai_tho" => LookupMark("้"),
		"hdr.mai_tri" => LookupMark("๊"),
		"hdr.mai_chattawa" => LookupMark("๋"),
		_ => null,
	};

	IConsonantClass? LookupClass(string code) =>
		_classesByCode.TryGetValue(code, out var c) ? c : null;

	IMark? LookupMark(string grapheme) =>
		_toneMarksByGrapheme.TryGetValue(grapheme, out var m) ? m : null;

	static string Capitalize(string s) =>
		string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];

	static string ClassCodeToOptionId(string code) => code switch
	{
		"low" => UiLabels.Options.ClassLow,
		"mid" => UiLabels.Options.ClassMid,
		"high" => UiLabels.Options.ClassHigh,
		_ => code,
	};

	static string? ToneKeyToId(string key) => key switch
	{
		"M" => UiLabels.Tone.Mid,
		"L" => UiLabels.Tone.Low,
		"F" => UiLabels.Tone.Falling,
		"H" => UiLabels.Tone.High,
		"R" => UiLabels.Tone.Rising,
		_ => null,
	};

	static ITranscription MergeTranscription(IReadOnlyList<IWord> words)
	{
		static string Join(IReadOnlyList<IWord> parts, Func<ITranscription, string?> pick) =>
			string.Join(" ", parts
				.Select(w => pick(w.Transcription))
				.Where(s => !string.IsNullOrWhiteSpace(s))!);

		return new Transcription(
			Join(words, t => t.Paiboon),
			Join(words, t => t.IPA));
	}

	static Dictionary<string, ToneRuleCell> BuildCells()
	{
		var d = new Dictionary<string, ToneRuleCell>(StringComparer.Ordinal);

		void Add(string id, string glyph = "", string toneKey = "")
		{
			d[id] = new ToneRuleCell
			{
				Id = id,
				TitleId = UiLabels.ToneRules.TitleId(id),
				DetailId = UiLabels.ToneRules.DetailId(id),
				Glyph = glyph,
				ToneKey = toneKey,
			};
		}

		Add("hdr.class");
		Add("hdr.syllable");
		Add("hdr.tonemark");
		Add("hdr.live");
		Add("hdr.dead");
		Add("hdr.long");
		Add("hdr.short");
		Add("hdr.mai_ek");
		Add("hdr.mai_tho");
		Add("hdr.mai_tri");
		Add("hdr.mai_chattawa");
		Add("hdr.low");
		Add("hdr.mid");
		Add("hdr.high");

		Add("low.live", "M", "M");
		Add("low.dead.long", "F", "F");
		Add("low.dead.short", "H", "H");
		Add("low.mai_ek", "F", "F");
		Add("low.mai_tho", "H", "H");

		Add("mid.live", "M", "M");
		Add("mid.dead", "L", "L");
		Add("mid.mai_ek", "L", "L");
		Add("mid.mai_tho", "F", "F");
		Add("mid.mai_tri", "H", "H");
		Add("mid.mai_chattawa", "R", "R");

		Add("high.live", "R", "R");
		Add("high.dead", "L", "L");
		Add("high.mai_ek", "L", "L");
		Add("high.mai_tho", "F", "F");

		return d;
	}
}
