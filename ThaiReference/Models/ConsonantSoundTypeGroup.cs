using CommunityToolkit.Mvvm.ComponentModel;

namespace ThaiReference.Models
{
    public partial class ConsonantSoundTypeGroup : ObservableObject
    {
        public ConsonantSoundTypeGroup(string soundType, IEnumerable<ConsonantSoundGroup> consonantSoundGroups)
        {
            SoundType = soundType;
            SoundTypeGroup = consonantSoundGroups;
        }
        public string SoundType { get; private set; }
        public IEnumerable<ConsonantSoundGroup> SoundTypeGroup { get; private set; }
        public int SoundCount => SoundTypeGroup.Count();
    }
}
