using CommunityToolkit.Mvvm.ComponentModel;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models
{
    public partial class ConsonantSoundGroup : ObservableObject
    {
        public ConsonantSoundGroup(ISound sound, IEnumerable<IConsonant> consonants)
        {
            Sound = sound;
            InitialConsonants = consonants.Where(x => x.InitialSound.Value == sound.Value);
            FinalConsonants = consonants.Where(x => x.EndingSound?.Value == sound.Value);
            AllConsonants = InitialConsonants.Concat(FinalConsonants);
        }
        public ISound Sound { get; set; }
        public IEnumerable<IConsonant> InitialConsonants { get; private set; }
        public IEnumerable<IConsonant> FinalConsonants { get; private set; }
        public IEnumerable<IConsonant> AllConsonants { get; private set; }
        public bool InitialAndFinal => InitialConsonants.Any() && FinalConsonants.Any();
        public bool OnlyInitial => !InitialAndFinal && InitialConsonants.Any();
        public bool OnlyFinal => !InitialAndFinal && FinalConsonants.Any();
        [ObservableProperty]
        bool visible = true;
    }
}
