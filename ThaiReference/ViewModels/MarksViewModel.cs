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

public partial class MarksViewModel : ObservableObject, IRomanizationReceiver, IVoiced
{
    public RelayCommand<object> PronounceCommand { get; init; }
    public IAudioManager AudioManager { get; init; }

    public ObservableCollection<IMark> ToneMarks { get; }
    public ObservableCollection<IMark> MiscMarks { get; }

    public MarksViewModel(IDataServiceFactory dsf, IAudioManager audioManager)
    {
        IDataService ds = dsf.Get("thai");
        WeakReferenceMessenger.Default.Register(this);
        AudioManager = audioManager;
        PronounceCommand = new RelayCommand<object>(async item => await this.Pronounce(item));

        var toneMarks = ds.Marks.Where(x => x.ToneMark && x.Words.Any()).ToList();
        ToneMarks = new ObservableCollection<IMark>(toneMarks);
        MiscMarks = new ObservableCollection<IMark>(ds.Marks.Where(x => !x.ToneMark));
    }

    [ObservableProperty]
    string romanizationOption = "IPA";

    public void Receive(RomanizationOptionChanged message) => RomanizationOption = message.Value;
}
