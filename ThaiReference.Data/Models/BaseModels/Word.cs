using ThaiReference.Contracts;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models.BaseModels;

public class Word : IAudible, IWord
{
    public Word() { }

    public Word(string value, ITranscription transcription, List<string> lexemes)
    {
        Value = value;
        Transcription = transcription;
        Lexemes = lexemes;
    }

    public ITranscription Transcription { get; set; } = null!;
    public string Value { get; set; } = "";
    public List<string> Lexemes { get; set; } = [];

    public string LexemesString => Lexemes.Count > 1 ? string.Concat("• ", string.Join("\n• ", Lexemes)) : Lexemes.FirstOrDefault() ?? "";
    public string Audible => Value;
}
