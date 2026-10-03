using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Audio;
using ThaiReference.Contracts;
using ThaiReference.DataAccess;
using ThaiReference.Messages;
using ThaiReference.Models.Interfaces;
using ThaiReference.Utilities;
using ThaiReference.Utilities.Extensions;

namespace ThaiReference.ViewModels;

public partial class HomophoneWordLineVm
{
    public required IWord Word { get; init; }
}

public partial class HomophoneGroupVm : ObservableObject
{
    public ITranscription? SharedTranscription { get; init; }
    public ObservableCollection<HomophoneWordLineVm> WordLines { get; init; } = [];

    // Every word in the group shares the same pronunciation, so one representative
    // word voices the whole group.
    public IWord? Sample => WordLines.FirstOrDefault()?.Word;

    public static HomophoneGroupVm FromModel(IHomophoneGroup group) => new()
    {
        SharedTranscription = group.Transcription,
        WordLines = new ObservableCollection<HomophoneWordLineVm>(
            group.Words.Select(w => new HomophoneWordLineVm { Word = w })),
    };
}

public partial class HomophonesViewModel : ObservableObject, IRomanizationReceiver, IVoiced
{
    public IAudioManager AudioManager { get; init; }
    public RelayCommand<object> PronounceCommand { get; init; }

    [ObservableProperty]
    string romanizationOption = "IPA";

    [ObservableProperty]
    ObservableCollection<HomophoneGroupVm> groups = [];

    public HomophonesViewModel(IDataServiceFactory dataServiceFactory, IAudioManager audioManager)
    {
        AudioManager = audioManager;
        WeakReferenceMessenger.Default.Register(this);
        PronounceCommand = new RelayCommand<object>(async o => await this.Pronounce(o));
        Groups = new ObservableCollection<HomophoneGroupVm>(
            dataServiceFactory.Get("thai").Homophones
                .Select(HomophoneGroupVm.FromModel)
                .OrderBy(g => g.WordLines.FirstOrDefault()?.Word.Value, StringComparer.Ordinal));
    }

    public void Receive(RomanizationOptionChanged message) => RomanizationOption = message.Value;
}
