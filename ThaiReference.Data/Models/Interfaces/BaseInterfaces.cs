using ThaiReference.Contracts;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Graphics;

namespace ThaiReference.Models.Interfaces;

public interface ISymbol
{
    string Grapheme { get; }
}

public interface IAlpha : ISymbol
{
    string Phoneme { get; }
}

public interface INumeral : ISymbol
{
    IWord Name { get; }
}

public interface IVowelGrapheme
{
    string Value { get; }
    bool IsClosed { get; }
}

public interface ISound : IHaveRecording, IHaveMediaUrl
{
    /// <summary>Canonical IPA label for this sound.</summary>
    string Value { get; }
    string SoundType { get; }
    bool IsConsonantSound { get; }
    bool IsInitialOnly { get; }
}

public interface ITranscription
{
    string? Paiboon { get; }
    string? IPA { get; }
    string Full { get; }
}

public interface IWord : IAudible
{
    ITranscription Transcription { get; }
    List<string> Lexemes { get; }
    string Value { get; }
    string LexemesString { get; }
}

public interface IMark : ISymbol, IAudible, IHaveRecording, IHaveDescription
{
    string Name { get; }
    string? EnglishLabel { get; }
    bool ToneMark { get; }
    IEnumerable<IWord> Words { get; }
}

/// <summary>Consonant class (อักษรต่ำ / กลาง / สูง); <see cref="Code"/> is low/mid/high.</summary>
public interface IConsonantClass : IAudible, IHaveRecording
{
    string Code { get; }
    string Name { get; }
    string? EnglishLabel { get; }
    IEnumerable<IWord> Words { get; }
}

public interface IConsonant : IAlpha, IAudible, IHaveDescription
{
    ITranscription Transcription { get; }
    IEnumerable<IWord> SampleWords { get; }
    ISound InitialSound { get; }
    ISound? EndingSound { get; }
    IWord? Acrophonic { get; }
    string Class { get; }
    string Ending { get; }
}

public interface IVowel : IHaveMediaUrl, IHaveDescription
{
    ISound Sound { get; }
    ITranscription Transcription { get; }
    IEnumerable<IVowelGrapheme> Graphemes { get; }
    IEnumerable<IWord> SampleWords { get; }
    string Phoneme { get; }
    bool IsDiphtong { get; }
    IEnumerable<string> GraphemesClosed { get; }
    IEnumerable<string> GraphemesOpen { get; }
    string GraphemeClosed { get; }
    string GraphemeOpen { get; }
    string GraphemesClosedString { get; }
    string GraphemesOpenString { get; }
    bool OnlyClosed { get; }
    bool OnlyOpen { get; }
    bool OpenAndClosed { get; }
}

public interface IPhoneticElement
{
    string DisplaySymbol { get; }
    Color FillColor { get; }
}

public interface IPronoun : IHaveDescription
{
    IWord Name { get; }
    string Formality { get; }
    string Gender { get; }
    int Person { get; }
    int Plurality { get; }
    IEnumerable<string> Types { get; }
}

public interface IGridPositionCell : IAudible
{
    int Ordinal { get; }
    int Row { get; }
    int Col { get; }
    string Caption { get; }
    string ItemNumber { get; }
    string? TileStyle { get; }
    string? Label { get; }
    IReadOnlyList<IWord> Words { get; }
}

public interface ISentence
{
    int Id { get; }
    string Translation { get; }
    IReadOnlyList<IWord> Words { get; }
    string Spelling { get; }
}

public interface IPosition : IAudible
{
    int Id { get; }
    string Lexeme { get; }
    IReadOnlyList<IWord> Words { get; }
    /// <summary>Concatenated head for display / TTS (CompositeName parts).</summary>
    IWord Word { get; }
    ISentence Example { get; }
}

public interface IWeekday : IAudible
{
    int Ordinal { get; }
    IWord Word { get; }
    string? Deity { get; }
    string? ImageUrl { get; }
}

/// <summary>Planet / luminary for a weekday; <see cref="Ordinal"/> matches <see cref="IWeekday.Ordinal"/>.
/// English gloss lives in <see cref="IWord.Lexemes"/> on <see cref="Word"/>.</summary>
public interface ICelestialBody
{
    int Ordinal { get; }
    string Glyph { get; }
    string ColorHex { get; }
    IWord Word { get; }
}

public interface ITimeOfDay : IAudible
{
    int Ordinal { get; }
    IWord Word { get; }
}

public interface IHour : IAudible
{
    /// <summary>0..23, mapping directly to clock-face hours.</summary>
    int Ordinal { get; }
    IWord Word { get; }
}

public interface ICluster
{
    string Type { get; }
    string? Subtype { get; }
    string Value { get; }
    IEnumerable<IWord> SampleWords { get; }
}

public interface IHomophoneGroup
{
    ITranscription Transcription { get; }
    IEnumerable<IWord> Words { get; }
}

/// <summary>A set of words that contrast on a single feature (tone or vowel length); the words
/// differ minimally, so reading their transcriptions side by side is the point. <see cref="Label"/>
/// names the contrast.</summary>
public interface IMinimalSet
{
    string Label { get; }
    IEnumerable<IWord> Words { get; }
}

public interface IZodiac
{
    int Ordinal { get; }
    string EnglishName { get; }
    string SanskritName { get; }
    string SanskritLexeme { get; }
    string Element { get; }
    string Glyph { get; }
}

public interface ISeason : IAudible
{
    int Ordinal { get; }
    IReadOnlyList<IWord> Words { get; }
    string Spelling { get; }
    ITranscription Transcription { get; }
    string EnglishLabel { get; }
    string Glyph { get; }
    IReadOnlyList<IMonth> Months { get; }
}

public interface IMonth : IAudible
{
    int Ordinal { get; }
    IWord Word { get; }
    string ShortName { get; }
    string EnglishName { get; }
    IZodiac Zodiac { get; }
}

/// <summary>Compass direction; <see cref="Degrees"/> is <c>(Id - 1) * 45</c> (N=0 … NW=315).</summary>
public interface IDirection : IAudible
{
    int Id { get; }
    string Label { get; }
    string Lexeme { get; }
    string Spelling { get; }
    ITranscription Transcription { get; }
    int Degrees { get; }
}

/// <summary>A single color with its Thai name built from constituent words.</summary>
public interface IColor : IAudible
{
    int Id { get; }
    string Hex { get; }
    string EnglishLabel { get; }
    int LuminanceOrder { get; }
    IReadOnlyList<IWord> Words { get; }
    string Spelling { get; }
    ITranscription Transcription { get; }
}

/// <summary>A hue family grouping related colors (e.g. reds, blues).</summary>
public interface IHueFamily
{
    int Id { get; }
    string ThaiName { get; }
    string EnglishName { get; }
    string Hex { get; }
    int DisplayOrder { get; }
    IReadOnlyList<IColor> Colors { get; }
}
