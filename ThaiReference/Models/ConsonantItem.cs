using CommunityToolkit.Mvvm.ComponentModel;
using ThaiReference.Contracts;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models
{
    public partial class ConsonantItem(IConsonant consonant) : ObservableObject, IConsonant
    {
        public IConsonant Consonant { get; init; } = consonant;

        public ISound InitialSound => Consonant.InitialSound;

        public ISound? EndingSound => Consonant.EndingSound;

        public ITranscription Transcription => Consonant.Transcription;

        public IEnumerable<IWord> SampleWords => Consonant.SampleWords;

        public IWord Acrophonic => Consonant.Acrophonic;

        public string Class => Consonant.Class;

        public string Ending => Consonant.Ending;

        public string Phoneme => Consonant.Phoneme;

        public string Grapheme => Consonant.Grapheme;

        string IAudible.Audible => Consonant.Audible;

        [ObservableProperty]
        public bool isVisible = true;
    }
}
