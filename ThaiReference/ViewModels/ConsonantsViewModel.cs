using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ThaiReference.Contracts;
using ThaiReference.Messages;
using ThaiReference.Ui;

namespace ThaiReference.ViewModels;

public partial class ConsonantsViewModel : ObservableObject, IRomanizationReceiver
{
	public partial class ConsonantViewOption : ObservableObject
	{
		public string LabelId { get; }
		public string TemplateKey { get; }
		public object ViewModel { get; }

		[ObservableProperty]
		string name = "";

		public ConsonantViewOption(string labelId, string templateKey, object viewModel)
		{
			LabelId = labelId;
			TemplateKey = templateKey;
			ViewModel = viewModel;
		}

		public void ApplyLanguage(UiLanguage language) =>
			Name = UiLabels.Get(LabelId, language);
	}

	[ObservableProperty]
	string romanizationOption = "IPA";

	[ObservableProperty]
	string viewOptionsTitle = "";

	[ObservableProperty]
	ConsonantViewOption selectedConsonantViewOption = null!;

	[ObservableProperty]
	List<ConsonantViewOption> consonantViewOptions = [];

	public ConsonantsViewModel(
		AllConsonantsViewModel all,
		ConsonantsBySoundViewModel bySound,
		ConsonantsBySoundTypeViewModel bySoundType)
	{
		WeakReferenceMessenger.Default.Register(this);
		WeakReferenceMessenger.Default.Register<UiLanguageChanged>(this, (_, m) => ApplyLanguage(m.Value));

		ConsonantViewOptions =
		[
			new(UiLabels.Options.List, "AllConsonants", all),
			new(UiLabels.Options.ListBySound, "ConsonantsBySound", bySound),
			new(UiLabels.Options.ListBySoundType, "ConsonantsBySoundType", bySoundType),
		];
		SelectedConsonantViewOption = ConsonantViewOptions[0];
		ApplyLanguage(UiLanguageHelper.Read());
	}

	public void Receive(RomanizationOptionChanged message) => RomanizationOption = message.Value;

	void ApplyLanguage(UiLanguage language)
	{
		ViewOptionsTitle = UiLabels.Get(UiLabels.Chrome.ViewOptions, language);
		foreach (var opt in ConsonantViewOptions)
			opt.ApplyLanguage(language);
		var selected = SelectedConsonantViewOption;
		ConsonantViewOptions = [.. ConsonantViewOptions];
		SelectedConsonantViewOption = selected;
	}
}
