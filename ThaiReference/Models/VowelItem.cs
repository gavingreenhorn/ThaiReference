using CommunityToolkit.Mvvm.ComponentModel;
using ThaiReference.Contracts;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models;

public partial class VowelItem(IVowel vowel) : ObservableObject, IVowel
{
    public IVowel Vowel { get; init; } = vowel;

    public ISound Sound => Vowel.Sound;

    public ITranscription Transcription => Vowel.Transcription;

    public IEnumerable<IVowelGrapheme> Graphemes => Vowel.Graphemes;

    public IEnumerable<IWord> SampleWords => Vowel.SampleWords;

    public string Phoneme => Vowel.Phoneme;

    public bool IsDiphtong => Vowel.IsDiphtong;

    public IEnumerable<string> GraphemesClosed => Vowel.GraphemesClosed;

    public IEnumerable<string> GraphemesOpen => Vowel.GraphemesOpen;

    public string GraphemeClosed => Vowel.GraphemeClosed;

    public string GraphemeOpen => Vowel.GraphemeOpen;

    public string GraphemesClosedString => Vowel.GraphemesClosedString;

    public string GraphemesOpenString => Vowel.GraphemesOpenString;

    public bool OnlyClosed => Vowel.OnlyClosed;

    public bool OnlyOpen => Vowel.OnlyOpen;

    public bool OpenAndClosed => Vowel.OpenAndClosed;

    public string? MediaUrl => Vowel.MediaUrl;

    [ObservableProperty]
    public bool isVisible = true;
}
