using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models;

public class Direction : IDirection
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Lexeme { get; set; } = string.Empty;
    public string Spelling { get; set; } = string.Empty;
    public ITranscription Transcription { get; set; } = null!;

    public int Degrees => (Id - 1) * 45;

    public string Audible => Spelling;
}
