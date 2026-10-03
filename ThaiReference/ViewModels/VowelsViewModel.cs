using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Audio;
using ThaiReference.Contracts;
using ThaiReference.Messages;
using ThaiReference.Ui;
using ThaiReference.Utilities.Extensions;
using ThaiReference.Views;

namespace ThaiReference.ViewModels
{
	public partial class VowelsViewModel : ObservableObject, IRomanizationReceiver, IVoiced, IDetailsNavigable
	{
		public RelayCommand<object> PronounceCommand { get; init; }
		public RelayCommand<IHaveDescription> ToDetailsCommand { get; init; }
		public IAudioManager AudioManager { get; init; }

		public partial class VowelViewOption : ObservableObject
		{
			public string LabelId { get; }
			public IContentView? View { get; set; }

			[ObservableProperty]
			string name = "";

			public VowelViewOption(string labelId, IContentView? view)
			{
				LabelId = labelId;
				View = view;
			}

			public void ApplyLanguage(UiLanguage language) =>
				Name = UiLabels.Get(LabelId, language);
		}

		[ObservableProperty]
		List<VowelViewOption> vowelViewOptions = null!;

		[ObservableProperty]
		VowelViewOption selectedVowelViewOption = null!;

		[ObservableProperty]
		string romanizationOption = "IPA";

		[ObservableProperty]
		string viewOptionsTitle = "";

		public VowelsViewModel(IAudioManager audioManager, AllVowels av, GraphemesBySound gbs)
		{
			WeakReferenceMessenger.Default.Register(this);
			WeakReferenceMessenger.Default.Register<UiLanguageChanged>(this, (_, m) => ApplyLanguage(m.Value));
			AudioManager = audioManager;
			PronounceCommand = new RelayCommand<object>(async (item) => await this.Pronounce(item!));
			ToDetailsCommand = new RelayCommand<IHaveDescription>(async (item) => await this.ToDetailsAsync(item!));
			VowelViewOptions =
			[
				new(UiLabels.Options.List, av),
				new(UiLabels.Options.ListBySound, gbs),
			];
			SelectedVowelViewOption = VowelViewOptions[0];
			ApplyLanguage(ReadUiLanguage());
		}

		public void Receive(RomanizationOptionChanged message) => RomanizationOption = message.Value;

		void ApplyLanguage(UiLanguage language)
		{
			ViewOptionsTitle = UiLabels.Get(UiLabels.Chrome.ViewOptions, language);
			foreach (var opt in VowelViewOptions)
				opt.ApplyLanguage(language);
			var selected = SelectedVowelViewOption;
			VowelViewOptions = [.. VowelViewOptions];
			SelectedVowelViewOption = selected;
		}

		static UiLanguage ReadUiLanguage() => UiLanguageHelper.Read();
	}
}
