using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;
using ThaiReference.Contracts;
using ThaiReference.DataAccess;
using ThaiReference.Messages;
using ThaiReference.Models;
using ThaiReference.Models.Interfaces;
using ThaiReference.Utilities;
using ThaiReference.Utilities.Extensions;

namespace ThaiReference.ViewModels;

public partial class ConsonantsBySoundTypeViewModel : ObservableObject, IVoiced, IRomanizationReceiver, IDetailsNavigable
{
    public RelayCommand<object> PronounceCommand { get; init; }
    public RelayCommand<IHaveDescription> ToDetailsCommand { get; init; }
    public IAudioManager AudioManager { get; init; }

    [ObservableProperty]
    string romanizationOption = "IPA";

    [ObservableProperty]
    ObservableCollection<ConsonantSoundTypeGroup> consonantSoundTypeGroups = [];

    public ConsonantsBySoundTypeViewModel(IDataServiceFactory dsf, IAudioManager audioManager)
    {
        IDataService ds = dsf.Get("thai");
        WeakReferenceMessenger.Default.Register(this);
        AudioManager = audioManager;
        PronounceCommand = new RelayCommand<object>(async (item) => await this.Pronounce(item));
        ToDetailsCommand = new RelayCommand<IHaveDescription>(async (item) => await this.ToDetailsAsync(item));

        var consonants = ds.Consonants.ToObservableCollection();
        var sounds = consonants
            .SelectMany(x => new[] { x.InitialSound, x.EndingSound! })
            .Where(x => x != null && !string.IsNullOrEmpty(x.Value))
            .DistinctBy(x => x.Value)
            .ToList();
        var soundGroups = sounds.Select(x => new ConsonantSoundGroup(x, consonants));
        ConsonantSoundTypeGroups = soundGroups
            .ToLookup(x => x.Sound.SoundType)
            .Select(x => new ConsonantSoundTypeGroup(x.Key, x))
            .ToObservableCollection();
    }

    public void Receive(RomanizationOptionChanged message)
    {
        RomanizationOption = message.Value;
    }
}
