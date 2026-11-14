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

public partial class MonthsViewModel : ObservableObject, IVoiced, IRomanizationReceiver
{
    public RelayCommand<object> PronounceCommand { get; init; }
    public IAudioManager AudioManager { get; init; }

    public IReadOnlyList<MonthWheelItemVm> Months { get; }

    private static readonly Dictionary<string, (string Light, string Dark)> ElementPalette =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Fire"]  = ElementHex("ElementFireLight", "ElementFireDark"),
            ["Earth"] = ElementHex("ElementEarthLight", "ElementEarthDark"),
            ["Air"]   = ElementHex("ElementAirLight", "ElementAirDark"),
            ["Water"] = ElementHex("ElementWaterLight", "ElementWaterDark"),
        };

    private static (string Light, string Dark) ElementHex(string lightKey, string darkKey) =>
        (ThemeColors.ToHex(ThemeColors.Get(lightKey)), ThemeColors.ToHex(ThemeColors.Get(darkKey)));

    [ObservableProperty] private string romanizationOption = "IPA";
    [ObservableProperty] private MonthWheelItemVm? selectedWheelItem;
    [ObservableProperty] private MonthPanelVm? selectedPanel;
    [ObservableProperty] private ISeason? selectedSeason;

    public MonthsViewModel(IDataServiceFactory dsf, IAudioManager audioManager)
    {
        IDataService ds = dsf.Get("thai");
        AudioManager = audioManager;
        WeakReferenceMessenger.Default.Register(this);
        PronounceCommand = new RelayCommand<object>(async item => await this.Pronounce(item));

        Seasons = ds.Seasons.OrderBy(s => s.Ordinal).ToList();
        Months = Seasons
            .SelectMany(s => s.Months.Select(m => CreateWheelItem(m, s.Ordinal)))
            .OrderBy(m => m.Ordinal)
            .ToList();
    }

    public IReadOnlyList<ISeason> Seasons { get; }

    partial void OnSelectedWheelItemChanged(MonthWheelItemVm? value)
    {
        if (value is null) return;
        SelectedSeason = null;
        SelectedPanel = CreatePanel(value);
    }

    [RelayCommand]
    private void SelectSeason(int ordinal)
    {
        var season = Seasons.FirstOrDefault(s => s.Ordinal == ordinal);
        if (season is null) return;
        SelectedWheelItem = null;
        SelectedSeason = season;
        SelectedPanel = new MonthPanelVm(
            season.Spelling,
            season.Transcription,
            season.EnglishLabel,
            season.EnglishLabel,
            string.Empty,
            season);
    }

    public void Receive(RomanizationOptionChanged message) => RomanizationOption = message.Value;

    [RelayCommand]
    private void SelectMonth(int ordinal)
    {
        SelectedWheelItem = Months.First(m => m.Ordinal == ordinal);
    }

    private static MonthWheelItemVm CreateWheelItem(IMonth m, int seasonOrdinal)
    {
        var palette = ElementPalette.TryGetValue(m.Zodiac?.Element ?? string.Empty, out var p)
            ? p
            : ElementPalette["Fire"];

        var en = m.EnglishName ?? string.Empty;
        string shortEnglish = en.Length >= 3 ? en[..3].ToLowerInvariant() : en.ToLowerInvariant();
        string glyph = m.Zodiac?.Glyph ?? string.Empty;

        return new MonthWheelItemVm(
            m,
            seasonOrdinal,
            m.ShortName ?? string.Empty,
            shortEnglish,
            glyph,
            palette.Light,
            palette.Dark);
    }

    private static MonthPanelVm CreatePanel(MonthWheelItemVm wheel)
    {
        var m = wheel.Month;
        string zodiacLine = $"{wheel.ZodiacSymbol} {m.Zodiac?.EnglishName ?? string.Empty}".Trim();
        string sanskrit = m.Zodiac is null
            ? string.Empty
            : $"{m.Zodiac.SanskritName} · {m.Zodiac.SanskritLexeme}".Trim(' ', '·');

        return new MonthPanelVm(
            m.Word?.Value ?? string.Empty,
            m.Word?.Transcription,
            m.EnglishName ?? string.Empty,
            zodiacLine,
            sanskrit,
            m);
    }

}
