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

    private static readonly string[] ZodiacGlyphByOrdinal =
    [
        "\u2651\uFE0E", "\u2652\uFE0E", "\u2653\uFE0E", "\u2648\uFE0E", "\u2649\uFE0E", "\u264A\uFE0E",
        "\u264B\uFE0E", "\u264C\uFE0E", "\u264D\uFE0E", "\u264E\uFE0E", "\u264F\uFE0E", "\u2650\uFE0E",
    ];

    [ObservableProperty] private string romanizationOption = "IPA";
    [ObservableProperty] private MonthWheelItemVm? selectedWheelItem;
    [ObservableProperty] private MonthPanelVm? selectedPanel;

    public MonthsViewModel(IDataServiceFactory dsf, IAudioManager audioManager)
    {
        IDataService ds = dsf.Get("thai");
        AudioManager = audioManager;
        WeakReferenceMessenger.Default.Register(this);
        PronounceCommand = new RelayCommand<object>(async item => await this.Pronounce(item));

        Months = ds.Months.OrderBy(m => m.Ordinal).Select(CreateWheelItem).ToList();
    }

    partial void OnSelectedWheelItemChanged(MonthWheelItemVm? value)
    {
        SelectedPanel = value is null ? null : CreatePanel(value);
    }

    public void Receive(RomanizationOptionChanged message) => RomanizationOption = message.Value;

    [RelayCommand]
    private void SelectMonth(int ordinal)
    {
        SelectedWheelItem = Months.First(m => m.Ordinal == ordinal);
    }

    private static MonthWheelItemVm CreateWheelItem(IMonth m)
    {
        var palette = ElementPalette.TryGetValue(m.Zodiac?.Element ?? string.Empty, out var p)
            ? p
            : ElementPalette["Fire"];

        var en = m.EnglishName ?? string.Empty;
        string shortEnglish = en.Length >= 3 ? en[..3].ToLowerInvariant() : en.ToLowerInvariant();
        string glyph = ZodiacGlyph(m.Zodiac);

        return new MonthWheelItemVm(
            m,
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

    private static string ZodiacGlyph(IZodiac? z)
    {
        if (z is null) return string.Empty;
        int ord = ((z.Ordinal % 12) + 12) % 12;
        return ZodiacGlyphByOrdinal[ord];
    }
}
