using ThaiReference.Contracts;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models.BaseModels;

public class Vowel : IVowel
{
    public bool IsDiphtong { get; set; }
    public ISound Sound { get; set; } = null!;
    public IEnumerable<IVowelGrapheme> Graphemes { get; set; } = [];
    public IEnumerable<IWord> SampleWords { get; set; } = [];
    public byte[]? Media { get; set; }
    public string? MediaUrl { get; set; }

    public IEnumerable<string> GraphemesClosed => Graphemes.Where(x => x.IsClosed).Select(x => x.Value);
    public IEnumerable<string> GraphemesOpen => Graphemes.Where(x => !x.IsClosed).Select(x => x.Value);
    public string GraphemeClosed => GraphemesClosed.FirstOrDefault() ?? "";
    public string GraphemeOpen => GraphemesOpen.FirstOrDefault() ?? "";
    public bool OpenAndClosed => GraphemesClosed.Any() && GraphemesOpen.Any();
    public bool OnlyOpen => !OpenAndClosed && GraphemesOpen.Any();
    public bool OnlyClosed => !OpenAndClosed && GraphemesClosed.Any();
    public string GraphemesClosedString => string.Join(Environment.NewLine, GraphemesClosed);
    public string GraphemesOpenString => string.Join(Environment.NewLine, GraphemesOpen);
    public ITranscription Transcription => new Transcription(Sound.Value, Sound.Value);
    public string Phoneme => Sound.Value;

    string? IHaveMediaUrl.MediaUrl => MediaUrl;
}
