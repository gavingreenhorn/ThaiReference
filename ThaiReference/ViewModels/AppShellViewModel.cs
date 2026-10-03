using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using ThaiReference.Messages;
using ThaiReference.Ui;

namespace ThaiReference.ViewModels
{
	public partial class AppShellViewModel : ObservableObject
	{
		const string PrefModern = "IsModern";

		[ObservableProperty]
		public bool isModern;

		[ObservableProperty]
		public ObservableCollection<string> romanizationOptions = [
			"IPA",
			"Paiboon"
		];

		[ObservableProperty]
		public string selectedRomanizationOption = null!;

		[ObservableProperty]
		public BaseShellItem shellCurrentItem = null!;

		[ObservableProperty]
		public ShellContent selectedThaiItem = null!;

		[ObservableProperty]
		public bool titleViewVisible = true;

		[ObservableProperty]
		public string modernFontLabel = "";

		[ObservableProperty]
		public string romanisationTitle = "";

		public IReadOnlyList<UiLanguageOption> UiLanguageOptions { get; } =
		[
			new(UiLanguage.English, "EN"),
			new(UiLanguage.Thai, "ไทย"),
		];

		[ObservableProperty]
		public UiLanguageOption selectedUiLanguageOption = null!;

		public UiLanguage UiLanguage => SelectedUiLanguageOption.Language;

		/// <summary>Invoked by <see cref="AppShell"/> after construction so titles can be applied to live Shell items.</summary>
		public Action? ApplyShellChrome { get; set; }

		public AppShellViewModel()
		{
			var modern = Preferences.Get(PrefModern, false);
			var lang = UiLanguageHelper.Read();

			// Set fields before property wiring so we don't persist/broadcast during DI construction.
			isModern = modern;
			selectedUiLanguageOption = UiLanguageOptions.First(o => o.Language == lang);
			SelectedRomanizationOption = RomanizationOptions[0];
			RefreshChromeLabels();
			SyncFontResources();
		}

		partial void OnSelectedRomanizationOptionChanged(string value)
		{
			WeakReferenceMessenger.Default.Send(new RomanizationOptionChanged(SelectedRomanizationOption));
		}

		partial void OnIsModernChanged(bool value)
		{
			Preferences.Set(PrefModern, value);
			SyncFontResources();
		}

		partial void OnSelectedUiLanguageOptionChanged(UiLanguageOption value)
		{
			if (value is null)
				return;
			UiLanguageHelper.Write(value.Language);
			UiLanguageState.Instance.Language = value.Language;
			RefreshChromeLabels();
			SyncFontResources();
			ApplyShellChrome?.Invoke();
			WeakReferenceMessenger.Default.Send(new UiLanguageChanged(value.Language));
		}

		void RefreshChromeLabels()
		{
			var lang = UiLanguage;
			UiLanguageState.Instance.Language = lang;
			ModernFontLabel = UiLabels.Get(UiLabels.Chrome.ModernFont, lang);
			RomanisationTitle = UiLabels.Get(UiLabels.Chrome.Romanisation, lang);
		}

		/// <summary>
		/// <c>CurrentThaiFont</c> always follows Modern (Thai orthography content).
		/// <c>CurrentChromeFont</c> follows Modern only when UI language is Thai;
		/// English chrome stays on <c>UIFont</c> (same split as VowelButton Sound → UIFont).
		/// </summary>
		void SyncFontResources()
		{
			if (Application.Current?.Resources is not { } resources)
				return;

			var thaiFont = IsModern ? "NotoSansThai" : "NotoSerifThai";
			resources["CurrentThaiFont"] = thaiFont;

			var uiFont = resources.TryGetValue("UIFont", out var ui) && ui is string s && !string.IsNullOrWhiteSpace(s)
				? s
				: "OpenSans";
			resources["CurrentChromeFont"] = UiLanguage == UiLanguage.Thai ? thaiFont : uiFont;
		}
	}
}
