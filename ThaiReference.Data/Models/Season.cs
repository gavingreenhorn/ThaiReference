using ThaiReference.Models.BaseModels;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models;

public class Season : ISeason
{
    public int Ordinal { get; set; }
    public IReadOnlyList<IWord> Words { get; set; } = [];
    public string EnglishLabel { get; set; } = string.Empty;
    public string Glyph { get; set; } = string.Empty;
    public IReadOnlyList<IMonth> Months { get; set; } = [];

    public string Spelling => string.Concat(Words.Select(w => w.Value));

    public ITranscription Transcription
    {
        get
        {
            if (Words.Count == 0)
                return new Transcription();
            if (Words.Count == 1)
                return Words[0].Transcription;

            string Join(Func<ITranscription, string?> pick) =>
                string.Join(" ", Words
                    .Select(w => pick(w.Transcription))
                    .Where(s => !string.IsNullOrWhiteSpace(s)));

            return new Transcription(
                Join(t => t.Paiboon),
                Join(t => t.IPA));
        }
    }

    public string Audible => Spelling;
}
