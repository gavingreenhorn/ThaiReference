using CommunityToolkit.Maui.Core.Extensions;
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

public partial class AllConsonantsViewModel : ObservableObject, IRomanizationReceiver, IVoiced, IDetailsNavigable
{
	public RelayCommand<object> PronounceCommand { get; init; }
	public RelayCommand<IHaveDescription> ToDetailsCommand { get; init; }
	public IAudioManager AudioManager { get; init; }

	public sealed class ClassOption
	{
		public required string Latin { get; init; }
		public required string LabelId { get; init; }
		public string? ThaiFromData { get; init; }
		/// <summary>Display caption bound by <c>OptionStrip</c> (<c>TextPropertyName="Thai"</c>).</summary>
		public string Thai { get; set; } = "";

		public void ApplyLanguage(UiLanguage language) =>
			Thai = UiLabels.ForStrip(LabelId, language, ThaiFromData);
	}

	public AllConsonantsViewModel(IDataServiceFactory dsf, IAudioManager audioManager)
	{
		IDataService ds = dsf.Get("thai");
		WeakReferenceMessenger.Default.Register(this);
		WeakReferenceMessenger.Default.Register<UiLanguageChanged>(this, (_, m) => ApplyLanguage(m.Value));
		ConsonantItems = ds.Consonants.Select(x => new ConsonantItem(x)).ToList();
		AudioManager = audioManager;
		PronounceCommand = new RelayCommand<object>(async (item) => await this.Pronounce(item));
		ToDetailsCommand = new RelayCommand<IHaveDescription>(async (item) => await this.ToDetailsAsync(item));
		DisplayedConsonants = ConsonantItems.ToObservableCollection();

		static string LabelIdFor(string code) => code switch
		{
			"low" => UiLabels.Options.ClassLow,
			"mid" => UiLabels.Options.ClassMid,
			"high" => UiLabels.Options.ClassHigh,
			_ => code,
		};

		var fromApi = ds.Classes
			.OrderBy(c => c.Code switch { "low" => 0, "mid" => 1, "high" => 2, _ => 9 })
			.Select(c => new ClassOption
			{
				Latin = c.Code,
				LabelId = LabelIdFor(c.Code),
				ThaiFromData = string.IsNullOrWhiteSpace(c.Name) ? null : c.Name,
			})
			.ToList();
		ClassOptions = (fromApi.Count > 0
			? fromApi
			:
			[
				new ClassOption { Latin = "low", LabelId = UiLabels.Options.ClassLow, ThaiFromData = "อักษรต่ำ" },
				new ClassOption { Latin = "mid", LabelId = UiLabels.Options.ClassMid, ThaiFromData = "อักษรกลาง" },
				new ClassOption { Latin = "high", LabelId = UiLabels.Options.ClassHigh, ThaiFromData = "อักษรสูง" },
			]).ToObservableCollection();

		ApplyLanguage(UiLanguageHelper.Read());
	}

	[ObservableProperty]
	string romanizationOption = "IPA";

	[ObservableProperty]
	ObservableCollection<ClassOption> classOptions = [];

	[ObservableProperty]
	List<ConsonantItem> consonantItems = [];

	[ObservableProperty]
	ObservableCollection<ConsonantItem> displayedConsonants = [];

	[ObservableProperty]
	ClassOption? selectedClassOption;

	partial void OnSelectedClassOptionChanged(ClassOption? value)
	{
		if (value is null || value.Latin == "all")
			DisplayedConsonants = ConsonantItems.ToObservableCollection();
		else
			DisplayedConsonants = ConsonantItems.Where(x => x.Class == value.Latin).ToObservableCollection();
	}

	public void Receive(RomanizationOptionChanged message) => RomanizationOption = message.Value;

	void ApplyLanguage(UiLanguage language)
	{
		var selected = SelectedClassOption;
		foreach (var o in ClassOptions)
			o.ApplyLanguage(language);
		ClassOptions = new ObservableCollection<ClassOption>(ClassOptions);
		SelectedClassOption = selected;
	}
}
