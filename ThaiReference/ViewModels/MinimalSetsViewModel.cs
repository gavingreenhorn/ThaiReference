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
    private const int PageSize = MinimalSetPage.DefaultPageSize;
    private readonly IDataService _ds;
    private int _currentPage = 1;

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

    [ObservableProperty]
    bool isPaged;

    [ObservableProperty]
    string pageRangeText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
    bool hasNextPage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
    bool hasPreviousPage;

    public MinimalSetsViewModel(IDataServiceFactory dataServiceFactory, IAudioManager audioManager)
    {
        AudioManager = audioManager;
        WeakReferenceMessenger.Default.Register(this);
        PronounceCommand = new RelayCommand<object>(async o => await this.Pronounce(o));

        _ds = dataServiceFactory.Get("thai");
        InitialSoundFilters = BuildInitialSoundFilters(_ds);
        FilterStripWidth = Math.Max(InitialSoundFilters.Count * FilterSegmentWidth, FilterSegmentWidth);
        LoadPage(1);
    }

    public void Receive(RomanizationOptionChanged message) => RomanizationOption = message.Value;

    partial void OnSelectedInitialSoundChanged(ISound? value)
    {
        LoadPage(1);
    }

    private string? SelectedLabel => SelectedInitialSound?.Value;

    private void LoadPage(int page)
    {
        int safePage = Math.Max(1, page);
        var result = _ds.MinimalSets(SelectedLabel, safePage, PageSize);
        var pageItems = result.Items
            .Select(MinimalSetVm.FromModel)
            .OrderBy(s => s.Label, StringComparer.Ordinal)
            .ThenBy(s => s.WordLines.FirstOrDefault()?.Word.Value, StringComparer.Ordinal)
            .ToList();

        Sets = new ObservableCollection<MinimalSetVm>(pageItems);
        _currentPage = safePage;

        long first = pageItems.Count == 0 ? 0 : (long)(safePage - 1) * PageSize + 1;
        long last = (long)(safePage - 1) * PageSize + pageItems.Count;
        PageRangeText = $"{first}-{last} / {result.TotalItems}";
        IsPaged = result.TotalItems > PageSize;
        HasPreviousPage = _currentPage > 1;
        HasNextPage = last < result.TotalItems;
    }

    private bool CanGoNext() => HasNextPage;
    private bool CanGoPrevious() => HasPreviousPage;

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void NextPage()
    {
        if (!HasNextPage)
            return;
        LoadPage(_currentPage + 1);
    }

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private void PreviousPage()
    {
        if (!HasPreviousPage)
            return;
        LoadPage(_currentPage - 1);
    }

    private static ObservableCollection<ISound> BuildInitialSoundFilters(IDataService ds)
    {
        var labels = ds.Consonants
            .Select(c => c.InitialSound)
            .Where(s => !string.IsNullOrWhiteSpace(s.Value))
            .GroupBy(s => s.Value, StringComparer.Ordinal)
            .Select(g => g.First())
            .OrderBy(s => s.Value, StringComparer.Ordinal)
            .Cast<ISound>()
            .ToList();

        return new ObservableCollection<ISound>(labels);
    }
}
