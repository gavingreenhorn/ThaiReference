namespace ThaiReference.Models
{
    public class WordLexemes
    {
        public int WordID { get; set; }
        public IEnumerable<string> Lexemes { get; set; } = [];
    }
}
