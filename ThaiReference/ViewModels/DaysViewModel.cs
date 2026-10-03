using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Audio;
using ThaiReference.Contracts;
using ThaiReference.DataAccess;
using ThaiReference.Messages;
using ThaiReference.Models;
using ThaiReference.Models.Interfaces;
using ThaiReference.Utilities;
using ThaiReference.Utilities.Extensions;

namespace ThaiReference.ViewModels;

public partial class DaysViewModel : ObservableObject, IVoiced, IRomanizationReceiver
{
    public RelayCommand<object> PronounceCommand { get; init; }
    public RelayCommand<WeekdayItem> SelectWeekdayCommand { get; init; }
    public IAudioManager AudioManager { get; init; }

    public ObservableCollection<WeekdayItem> WeekdayItems { get; init; }

    public DaysViewModel(IDataServiceFactory dsf, IAudioManager audioManager)
    {
        IDataService ds = dsf.Get("thai");
        AudioManager = audioManager;
        WeakReferenceMessenger.Default.Register(this);
        PronounceCommand = new RelayCommand<object>(async item => await this.Pronounce(item));
        SelectWeekdayCommand = new RelayCommand<WeekdayItem>(item => SelectedItem = item);
        Dictionary<int, ICelestialBody> cbLookup = ds.CelestialBodies.ToDictionary(cb => cb.Ordinal);
        WeekdayItems = ds.Weekdays
            .OrderBy(wd => wd.Ordinal)
            .Select(wd => (wd, pl: cbLookup[wd.Ordinal]))
            .Select(x => new WeekdayItem(x.wd, x.pl))
            .ToObservableCollection();
    } 

    public void Receive(RomanizationOptionChanged message) => RomanizationOption = message.Value;

    [ObservableProperty]
    private string romanizationOption = "IPA";

    [ObservableProperty]
    private WeekdayItem? selectedItem;

    partial void OnSelectedItemChanged(WeekdayItem? value)
    {
        if (value == null) return;
        foreach (var i in WeekdayItems)
            i.IsSelected = i == value;
    }
}
