using CommunityToolkit.Mvvm.ComponentModel;
using ThaiReference.DataAccess;
using ThaiReference.Utilities;
using System.Collections.ObjectModel;
using ThaiReference.Models;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.Messaging;
using ThaiReference.Messages;
using ThaiReference.Ui;

namespace ThaiReference.ViewModels;

public partial class AllVowelsViewModel : ObservableObject
{
	public sealed class SoundTypeOption
	{
		public required string Latin { get; init; }
		public required string LabelId { get; init; }
		public string? ThaiFromData { get; init; }
		public string Thai { get; set; } = "";

		public void ApplyLanguage(UiLanguage language) =>
			Thai = UiLabels.ForStrip(LabelId, language, ThaiFromData);
	}

	public sealed class VowelVisualizationOption
	{
		public required string Latin { get; init; }
		public required string LabelId { get; init; }
		public string? ThaiFromData { get; init; }
		public string Thai { get; set; } = "";

		public void ApplyLanguage(UiLanguage language) =>
			Thai = UiLabels.ForStrip(LabelId, language, ThaiFromData);
	}

	public AllVowelsViewModel(IDataServiceFactory dsf)
	{
		IDataService ds = dsf.Get("thai");
		WeakReferenceMessenger.Default.Register<UiLanguageChanged>(this, (_, m) => ApplyLanguage(m.Value));
		VowelItems = [.. ds.Vowels.Select(x => new VowelItem(x))];
		DisplayedVowels = new ObservableCollection<VowelItem>(VowelItems);
		SoundTypeOptions =
		[
			new() { Latin = "all", LabelId = UiLabels.Options.SoundAll, ThaiFromData = "เสียง-ทั้งหมด" },
			new() { Latin = "short", LabelId = UiLabels.Options.SoundShort, ThaiFromData = "สระ-เสียง-สั้น" },
			new() { Latin = "long", LabelId = UiLabels.Options.SoundLong, ThaiFromData = "สระ-เสียง-ยาว" },
		];
		VowelVisualizationOptions =
		[
			new() { Latin = "Sound", LabelId = UiLabels.Options.VizSound, ThaiFromData = "สระ-เสียง" },
			new() { Latin = "Open", LabelId = UiLabels.Options.VizOpen, ThaiFromData = "คำ-เป็น" },
			new() { Latin = "Closed", LabelId = UiLabels.Options.VizClosed, ThaiFromData = "คำ-ตาย" },
		];
		ApplyLanguage(UiLanguageHelper.Read());
		SelectedSoundTypeOption = SoundTypeOptions[0];
		SelectedVowelVisualizationOption = VowelVisualizationOptions[0];
	}

	[ObservableProperty]
	List<VowelItem> vowelItems = [];

	[ObservableProperty]
	ObservableCollection<VowelItem> displayedVowels = [];

	[ObservableProperty]
	SoundTypeOption selectedSoundTypeOption = null!;

	[ObservableProperty]
	VowelVisualizationOption selectedVowelVisualizationOption = null!;

	[ObservableProperty]
	ObservableCollection<SoundTypeOption> soundTypeOptions = [];

	[ObservableProperty]
	List<VowelVisualizationOption> vowelVisualizationOptions = [];

	partial void OnSelectedSoundTypeOptionChanged(SoundTypeOption value)
	{
		if (value == null || value.Latin == "all")
			DisplayedVowels = new ObservableCollection<VowelItem>(VowelItems);
		else
			DisplayedVowels = VowelItems.Where(x => x.Sound.SoundType == value.Latin).ToObservableCollection();
	}

	void ApplyLanguage(UiLanguage language)
	{
		var soundSel = SelectedSoundTypeOption;
		var vizSel = SelectedVowelVisualizationOption;
		foreach (var o in SoundTypeOptions)
			o.ApplyLanguage(language);
		foreach (var o in VowelVisualizationOptions)
			o.ApplyLanguage(language);
		SoundTypeOptions = new ObservableCollection<SoundTypeOption>(SoundTypeOptions);
		VowelVisualizationOptions = [.. VowelVisualizationOptions];
		SelectedSoundTypeOption = soundSel;
		SelectedVowelVisualizationOption = vizSel;
	}
}
