using ThaiReference.Contracts;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.ViewModels;

/// <summary>Bottom panel row for the clock; built in <see cref="ThaiClockViewModel"/> when a band or hour is selected.</summary>
public sealed class ThaiClockPanelVm
{
    public string Thai { get; }
    public ITranscription? Transcription { get; }
    public IAudible PronounceTarget { get; }

    public ThaiClockPanelVm(
        string thai,
        ITranscription? transcription,
        IAudible pronounceTarget)
    {
        Thai = thai;
        Transcription = transcription;
        PronounceTarget = pronounceTarget;
    }
}
