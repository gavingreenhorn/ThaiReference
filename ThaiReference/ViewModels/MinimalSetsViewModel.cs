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

public partial class MinimalSetWordVm
{
    public required IWord Word { get; init; }
}

public partial class MinimalSetVm : ObservableObject
{
    internal string Label { get; init; } = "";
    public ObservableCollection<MinimalSetWordVm> WordLines { get; init; } = [];

    public static MinimalSetVm FromModel(IMinimalSet set) => new()
    {
        Label = set.Label,
        WordLines = new ObservableCollection<MinimalSetWordVm>(
            set.Words.Select(w => new MinimalSetWordVm { Word = w })),
    };
}

public partial class MinimalSetsViewModel : ObservableObject, IRomanizationReceiver, IVoiced
{
    private const double FilterSegmentWidth = 48;
    private readonly List<MinimalSetVm> _allSets = [];

    public IAudioManager AudioManager { get; init; }
    public RelayCommand<object> PronounceCommand { get; init; }

    [ObservableProperty]
    string romanizationOption = "IPA";

    [ObservableProperty]
    ObservableCollection<MinimalSetVm> sets = [];

    [ObservableProperty]
    ObservableCollection<ISound> initialSoundFilters = [];

    [ObservableProperty]
    ISound? selectedInitialSound;

    [ObservableProperty]
    double filterStripWidth;

    public MinimalSetsViewModel(IDataServiceFactory dataServiceFactory, IAudioManager audioManager)
    {
        AudioManager = audioManager;
        WeakReferenceMessenger.Default.Register(this);
        PronounceCommand = new RelayCommand<object>(async o => await this.Pronounce(o));

        var ds = dataServiceFactory.Get("thai");
        _allSets = ds.MinimalSets
            .Select(MinimalSetVm.FromModel)
            .OrderBy(s => s.Label, StringComparer.Ordinal)
            .ThenBy(s => s.WordLines.FirstOrDefault()?.Word.Value, StringComparer.Ordinal)
            .ToList();

        InitialSoundFilters = BuildInitialSoundFilters(ds, _allSets);
        FilterStripWidth = Math.Max(InitialSoundFilters.Count * FilterSegmentWidth, FilterSegmentWidth);
        ApplyFilter();
    }

    public void Receive(RomanizationOptionChanged message) => RomanizationOption = message.Value;

    partial void OnSelectedInitialSoundChanged(ISound? value) => ApplyFilter();

    private void ApplyFilter()
    {
        IEnumerable<MinimalSetVm> visible = _allSets;
        if (SelectedInitialSound is not null)
        {
            var label = SelectedInitialSound.Value;
            visible = _allSets.Where(s => string.Equals(s.Label, label, StringComparison.Ordinal));
        }

        Sets = new ObservableCollection<MinimalSetVm>(visible);
    }

    private static ObservableCollection<ISound> BuildInitialSoundFilters(
        IDataService ds,
        IReadOnlyList<MinimalSetVm> sets)
    {
        var labels = sets
            .Select(s => s.Label)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToHashSet(StringComparer.Ordinal);

        var soundByValue = ds.Consonants
            .Select(c => c.InitialSound)
            .Where(s => !string.IsNullOrWhiteSpace(s.Value))
            .GroupBy(s => s.Value, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        var filters = labels
            .Where(soundByValue.ContainsKey)
            .Select(label => soundByValue[label])
            .OrderBy(s => s.Value, StringComparer.Ordinal)
            .Cast<ISound>()
            .ToList();

        return new ObservableCollection<ISound>(filters);
    }
}
