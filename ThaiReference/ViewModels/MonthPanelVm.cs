using ThaiReference.Contracts;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.ViewModels;

/// <summary>Bottom panel for months; built from <see cref="MonthWheelItemVm"/> in <see cref="MonthsViewModel"/>.</summary>
public sealed class MonthPanelVm
{
    public string Thai { get; }
    public ITranscription? Transcription { get; }
    public string Meaning { get; }
    public string ZodiacLine { get; }
    public string SanskritLine { get; }
    public IAudible PronounceTarget { get; }

    public MonthPanelVm(
        string thai,
        ITranscription? transcription,
        string meaning,
        string zodiacLine,
        string sanskritLine,
        IAudible pronounceTarget)
    {
        Thai = thai;
        Transcription = transcription;
        Meaning = meaning;
        ZodiacLine = zodiacLine;
        SanskritLine = sanskritLine;
        PronounceTarget = pronounceTarget;
    }
}
