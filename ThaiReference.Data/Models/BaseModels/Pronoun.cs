using ThaiReference.Contracts;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models.BaseModels;

public class Pronoun : IHaveDescription, IPronoun
{
    public IWord Name { get; set; } = null!;
    public int Person { get; set; }
    public int Plurality { get; set; }
    public string Formality { get; set; } = "";
    public string Gender { get; set; } = "";
    public IEnumerable<string> Types { get; set; } = [];
}
