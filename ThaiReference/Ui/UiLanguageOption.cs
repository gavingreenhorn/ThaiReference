using ThaiReference.Ui;

namespace ThaiReference.Ui;

/// <summary>Title-bar segment for UI language (<c>OptionStrip</c> binds <see cref="Thai"/>).</summary>
public sealed class UiLanguageOption(UiLanguage language, string thai)
{
	public UiLanguage Language { get; } = language;
	public string Thai { get; } = thai;
}
