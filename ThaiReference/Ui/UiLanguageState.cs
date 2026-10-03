using CommunityToolkit.Mvvm.ComponentModel;

namespace ThaiReference.Ui;

/// <summary>App-wide UI language for bindings. Updated when the user changes language.</summary>
public partial class UiLanguageState : ObservableObject
{
	public static UiLanguageState Instance { get; } = new();

	[ObservableProperty]
	UiLanguage language = UiLanguageHelper.Read();
}
