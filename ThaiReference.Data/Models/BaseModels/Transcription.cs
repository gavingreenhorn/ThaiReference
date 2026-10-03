using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models.BaseModels;

public class Transcription(string paiboon = "", string ipa = "") : ITranscription
{
    public int TranscriptionID { get; set; }
    public string? Paiboon { get; set; } = paiboon;
    public string? IPA { get; set; } = ipa;

    public string Full
    {
        get
        {
            if (!(string.IsNullOrEmpty(IPA) || string.IsNullOrEmpty(Paiboon)))
            {
                return IPA == Paiboon ? IPA : string.Concat(IPA, " | ", Paiboon);
            }
            else if (!string.IsNullOrEmpty(IPA)) { return IPA; }
            else if (!string.IsNullOrEmpty(Paiboon)) { return Paiboon; }
            else return string.Empty;
        }
    }
}
