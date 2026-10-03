using CommunityToolkit.Mvvm.ComponentModel;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models;

public partial class PronounItem(IPronoun pronoun) : ObservableObject, IPronoun
{
    public IPronoun Pronoun { get; init; } = pronoun;

    public IWord Name => Pronoun.Name;

    public string Formality => Pronoun.Formality;

    public string Gender => Pronoun.Gender;

    public int Person => Pronoun.Person;

    public int Plurality => Pronoun.Plurality;

    public IEnumerable<string> Types => Pronoun.Types;

    [ObservableProperty]
    bool isVisible = true;
}
