using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models;

public class PronounTypeGroup(string typeName, ILookup<int, PronounItem> group)
{
    public string TypeName { get; init; } = typeName;
    public ILookup<int, PronounItem> Group { get; init; } = group;
}
