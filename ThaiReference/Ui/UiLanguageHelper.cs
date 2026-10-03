namespace ThaiReference.Ui;

/// <summary>Shared helpers for UI language preference and strip caption updates.</summary>
public static class UiLanguageHelper
{
	public const string PrefKey = "UiLanguage";

	public static UiLanguage Read()
	{
		var name = Preferences.Get(PrefKey, nameof(UiLanguage.English));
		return Enum.TryParse<UiLanguage>(name, ignoreCase: true, out var lang) ? lang : UiLanguage.English;
	}

	public static void Write(UiLanguage language) =>
		Preferences.Set(PrefKey, language.ToString());
}
