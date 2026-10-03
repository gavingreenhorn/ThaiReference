using ThaiReference.Contracts;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models.BaseModels;

public class Consonant : IConsonant
{
    public string Phoneme { get; set; } = "";
    public string Grapheme { get; set; } = "";
    public ITranscription Transcription { get; set; } = null!;
    public ISound InitialSound { get; set; } = null!;
    public ISound? EndingSound { get; set; }
    public string Class { get; set; } = "";
    public string Ending { get; set; } = "";
    public IEnumerable<IWord> SampleWords { get; set; } = [];
    public IWord? Acrophonic => SampleWords.FirstOrDefault();

    string IAudible.Audible => string.Concat(Grapheme, " ", Acrophonic?.Audible ?? "");
}
