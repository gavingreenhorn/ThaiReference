using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;
using ThaiReference.Contracts;
using ThaiReference.DataAccess;
using ThaiReference.Messages;
using ThaiReference.Models;
using ThaiReference.Models.Interfaces;
using ThaiReference.Ui;
using ThaiReference.Utilities;
using ThaiReference.Utilities.Extensions;

namespace ThaiReference.ViewModels;

public partial class ConsonantsBySoundViewModel : ObservableObject, IVoiced, IRomanizationReceiver, IDetailsNavigable
{
	public RelayCommand<object> PronounceCommand { get; init; }
	public RelayCommand<IHaveDescription> ToDetailsCommand { get; init; }
	public IAudioManager AudioManager { get; init; }

	public sealed class ConsonantPosition
	{
		public required string Latin { get; init; }
		public required string LabelId { get; init; }
		public string? ThaiFromData { get; init; }
		public string Thai { get; set; } = "";

		public void ApplyLanguage(UiLanguage language) =>
			Thai = UiLabels.ForStrip(LabelId, language, ThaiFromData);
	}

	[ObservableProperty]
	string romanizationOption = "IPA";

	[ObservableProperty]
	ConsonantPosition? selectedConsonantPosition;

	[ObservableProperty]
	ObservableCollection<ConsonantSoundGroup> consonantSoundGroups = [];

	[ObservableProperty]
	ObservableCollection<ConsonantPosition> consonantPositions = [];

	public ConsonantsBySoundViewModel(IDataServiceFactory dsf, IAudioManager audioManager)
	{
		IDataService ds = dsf.Get("thai");
		WeakReferenceMessenger.Default.Register(this);
		WeakReferenceMessenger.Default.Register<UiLanguageChanged>(this, (_, m) => ApplyLanguage(m.Value));
		AudioManager = audioManager;
		PronounceCommand = new RelayCommand<object>(async (item) => await this.Pronounce(item));
		ToDetailsCommand = new RelayCommand<IHaveDescription>(async (item) => await this.ToDetailsAsync(item));

		ConsonantPositions =
		[
			new() { Latin = "initial", LabelId = UiLabels.Options.PosInitial, ThaiFromData = "พยัญชนะ-ต้น" },
			new() { Latin = "final", LabelId = UiLabels.Options.PosFinal, ThaiFromData = "พยัญชนะ-ท้าย" },
			new() { Latin = "initial and final", LabelId = UiLabels.Options.PosInitialAndFinal, ThaiFromData = "พยัญชนะ-ต้น-และ-ท้าย" },
		];

		var consonants = ds.Consonants.ToObservableCollection();
		var sounds = consonants
			.SelectMany(x => new[] { x.InitialSound, x.EndingSound! })
			.Where(x => x != null && !string.IsNullOrEmpty(x.Value))
			.DistinctBy(x => x.Value)
			.ToList();
		ConsonantSoundGroups = sounds.Select(x => new ConsonantSoundGroup(x, consonants)).ToObservableCollection();
		ApplyLanguage(UiLanguageHelper.Read());
	}

	public void Receive(RomanizationOptionChanged message) => RomanizationOption = message.Value;

	partial void OnSelectedConsonantPositionChanged(ConsonantPosition? value)
	{
		if (value is null || value.Latin == "all")
			foreach (var item in ConsonantSoundGroups)
				item.Visible = true;
		else if (value.Latin == "initial")
			foreach (var item in ConsonantSoundGroups)
				item.Visible = item.OnlyInitial;
		else if (value.Latin == "final")
			foreach (var item in ConsonantSoundGroups)
				item.Visible = item.OnlyFinal;
		else
			foreach (var item in ConsonantSoundGroups)
				item.Visible = item.InitialAndFinal;
	}

	void ApplyLanguage(UiLanguage language)
	{
		var selected = SelectedConsonantPosition;
		foreach (var o in ConsonantPositions)
			o.ApplyLanguage(language);
		ConsonantPositions = new ObservableCollection<ConsonantPosition>(ConsonantPositions);
		SelectedConsonantPosition = selected;
	}
}
