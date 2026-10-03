using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ThaiReference.DataAccess;
using ThaiReference.Contracts;
using ThaiReference.Messages;
using ThaiReference.Models;
using ThaiReference.Ui;
using ThaiReference.Utilities;
using ThaiReference.Utilities.Extensions;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;

namespace ThaiReference.ViewModels;

public partial class PronounsViewModel : ObservableObject, IVoiced, IRomanizationReceiver
{
	public RelayCommand<object> PronounceCommand { get; init; }
	public IAudioManager AudioManager { get; init; }

	public sealed class PluralityOption
	{
		public required string Latin { get; init; }
		public required string LabelId { get; init; }
		public string? ThaiFromData { get; init; }
		public string Thai { get; set; } = "";

		public void ApplyLanguage(UiLanguage language) =>
			Thai = UiLabels.ForStrip(LabelId, language, ThaiFromData);
	}

	public sealed class GrammaticalPersonOption
	{
		public required int Indexer { get; init; }
		public required string Latin { get; init; }
		public required string LabelId { get; init; }
		public string? ThaiFromData { get; init; }
		public string Thai { get; set; } = "";

		public void ApplyLanguage(UiLanguage language) =>
			Thai = UiLabels.ForStrip(LabelId, language, ThaiFromData);
	}

	public PronounsViewModel(IDataServiceFactory dsf, IAudioManager audioManager)
	{
		IDataService ds = dsf.Get("thai");
		AudioManager = audioManager;
		WeakReferenceMessenger.Default.Register(this);
		WeakReferenceMessenger.Default.Register<UiLanguageChanged>(this, (_, m) => ApplyLanguage(m.Value));
		PronounItems = ds.Pronouns.Select(x => new PronounItem(x)).ToList();
		PronounGroups = new(PronounItems.SelectMany(x => x.Types).Distinct().Select(GetPronounGrouping));
		PronounceCommand = new RelayCommand<object>(async (item) => await this.Pronounce(item));

		PluralityOptions =
		[
			new() { Latin = "Singular", LabelId = UiLabels.Options.PluralitySingular, ThaiFromData = "เอกพจน์" },
			new() { Latin = "All", LabelId = UiLabels.Options.PluralityAll, ThaiFromData = "ทั้งหมด" },
			new() { Latin = "Plural", LabelId = UiLabels.Options.PluralityPlural, ThaiFromData = "พหูพจน์" },
		];
		GrammaticalPersonOptions =
		[
			new() { Indexer = 1, Latin = "First", LabelId = UiLabels.Options.PersonFirst, ThaiFromData = "บุรุษที่หนึ่ง" },
			new() { Indexer = 2, Latin = "Second", LabelId = UiLabels.Options.PersonSecond, ThaiFromData = "บุรุษที่สอง" },
			new() { Indexer = 3, Latin = "Third", LabelId = UiLabels.Options.PersonThird, ThaiFromData = "บุรุษที่สาม" },
		];
		ApplyLanguage(UiLanguageHelper.Read());
	}

	public List<PronounItem> PronounItems { get; set; }

	[ObservableProperty]
	string romanizationOption = "IPA";

	[ObservableProperty]
	PluralityOption selectedPluralityOption = null!;

	[ObservableProperty]
	List<PronounTypeGroup> pronounGroups = null!;

	[ObservableProperty]
	ObservableCollection<PluralityOption> pluralityOptions = [];

	[ObservableProperty]
	ObservableCollection<GrammaticalPersonOption> grammaticalPersonOptions = [];

	partial void OnSelectedPluralityOptionChanged(PluralityOption value)
	{
		if (value is null)
			return;
		foreach (var item in PronounItems)
		{
			item.IsVisible =
				value.Latin == "All" ||
				item.Plurality == 3 ||
				item.Plurality == 1 && value.Latin == "Singular" ||
				item.Plurality == 2 && value.Latin == "Plural";
		}
	}

	private PronounTypeGroup GetPronounGrouping(string pronounType)
	{
		return new PronounTypeGroup(
			typeName: pronounType,
			group: PronounItems.Where(x => x.Types.Contains(pronounType)).ToLookup(x => x.Person)
		);
	}

	public void Receive(RomanizationOptionChanged message) => RomanizationOption = message.Value;

	void ApplyLanguage(UiLanguage language)
	{
		var plurality = SelectedPluralityOption;
		foreach (var o in PluralityOptions)
			o.ApplyLanguage(language);
		foreach (var o in GrammaticalPersonOptions)
			o.ApplyLanguage(language);
		PluralityOptions = new ObservableCollection<PluralityOption>(PluralityOptions);
		GrammaticalPersonOptions = new ObservableCollection<GrammaticalPersonOption>(GrammaticalPersonOptions);
		if (plurality is not null)
			SelectedPluralityOption = plurality;
	}
}
