using ThaiReference.Models.BaseModels;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models;

public class Sentence : ISentence
{
    public int Id { get; set; }
    public string Translation { get; set; } = string.Empty;
    public List<Word> Words { get; set; } = [];

    IReadOnlyList<IWord> ISentence.Words => Words;

    public string Spelling => string.Concat(Words.Select(w => w.Value));
}
