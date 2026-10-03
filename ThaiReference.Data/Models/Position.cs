using ThaiReference.Models.BaseModels;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models;

public class Position : IPosition
{
    public int Id { get; set; }
    public string Lexeme { get; set; } = string.Empty;
    public List<Word> Words { get; set; } = [];
    public Word Word { get; set; } = null!;
    public Sentence Example { get; set; } = null!;

    IReadOnlyList<IWord> IPosition.Words => Words;
    IWord IPosition.Word => Word;
    ISentence IPosition.Example => Example;

    public string Audible => Word?.Value ?? string.Empty;
}
