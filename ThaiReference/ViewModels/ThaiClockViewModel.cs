using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Audio;
using ThaiReference.Contracts;
using ThaiReference.DataAccess;
using ThaiReference.Messages;
using ThaiReference.Models;
using ThaiReference.Models.BaseModels;
using ThaiReference.Models.Interfaces;
using ThaiReference.Utilities;
using ThaiReference.Utilities.Extensions;

namespace ThaiReference.ViewModels;

public partial class ThaiClockViewModel : ObservableObject, IVoiced, IRomanizationReceiver
{
    public const int SaaiOrdinal = 7;

    public RelayCommand<object> PronounceCommand { get; init; }
    public IAudioManager AudioManager { get; init; }

    public IReadOnlyList<ClockStripSegmentVm> TimeOfDaySegments { get; }

    /// <summary>Ordinal → inclusive hour band + Colors.xaml key.</summary>
    private static readonly Dictionary<int, (int Start, int End, string ColorKey)> Bands = new()
    {
        [0] = (0, 0, "ClockThiangKhuen"),
        [1] = (1, 5, "ClockDtii"),
        [2] = (6, 8, "ClockChao"),
        [SaaiOrdinal] = (9, 11, "ClockSaai"),
        [3] = (12, 12, "ClockThiang"),
        [4] = (13, 15, "ClockBaai"),
        [5] = (16, 18, "ClockYen"),
        [6] = (19, 23, "ClockKham"),
    };

    private readonly Dictionary<int, ITimeOfDay> _timeOfDayByOrdinal = [];
    private readonly Dictionary<int, IHour> _hourByOrdinal = [];
    private readonly ThaiNumber[] _minutes;

    public ThaiClockViewModel(IDataServiceFactory dsf, IAudioManager audioManager, INumeral[] numerals)
    {
        IDataService ds = dsf.Get("thai");
        AudioManager = audioManager;
        WeakReferenceMessenger.Default.Register(this);
        PronounceCommand = new RelayCommand<object>(async item => await this.Pronounce(item));

        // Same composition engine as NumbersView — cache 0..60 once for minute lookup.
        _minutes = Enumerable.Range(0, 61).Select(m => new ThaiNumber(m, numerals)).ToArray();

        foreach (var item in ds.TimeOfDayItems)
            _timeOfDayByOrdinal[item.Ordinal] = item;

        foreach (var item in ds.HourItems)
            _hourByOrdinal[item.Ordinal] = item;

        TimeOfDaySegments = Bands
            .OrderBy(kv => kv.Value.Start)
            .Select(kv =>
            {
                var tod = _timeOfDayByOrdinal[kv.Key];
                var (start, end, colorKey) = kv.Value;
                return new ClockStripSegmentVm(
                    kv.Key,
                    tod.Word?.Value ?? string.Empty,
                    start,
                    end,
                    ThemeColors.ToHex(ThemeColors.Get(colorKey)));
            })
            .ToList();

        SelectHour(DateTime.Now.Hour);
    }

    [ObservableProperty] private string romanizationOption = "IPA";
    [ObservableProperty] private int selectedOuterOrdinal = -1;
    [ObservableProperty] private int selectedInnerHour = -1;
    [ObservableProperty] private int selectedDayNightSegment = -1;
    [ObservableProperty] private bool isCenterSelected;
    [ObservableProperty] private ThaiClockPanelVm? selectedPanel;

    /// <summary>Live 24h HH:MM shown on the LCD watchface; refreshed by <see cref="Tick"/>.</summary>
    [ObservableProperty] private string currentTimeText = DateTime.Now.ToString("HH:mm");

    private int _lastMinute = -1;

    /// <summary>Called once per second by the view timer; updates the watchface and,
    /// when an hour is selected, the minute portion of the detail strip.</summary>
    public void Tick()
    {
        var now = DateTime.Now;
        int hour = SelectedInnerHour >= 0 ? SelectedInnerHour : now.Hour;
        CurrentTimeText = $"{hour:00}:{now.Minute:00}";
        if (now.Minute != _lastMinute)
        {
            _lastMinute = now.Minute;
            if (SelectedInnerHour >= 0)
                SelectedPanel = BuildHourPanel(SelectedInnerHour, now.Minute);
        }
    }

    private sealed class ClockAudible(string text) : IAudible
    {
        public string Audible => text;
    }

    /// <summary>Hardcoded "minute" word; reacts to transcription switching like any other word.</summary>
    private static IWord NateeWord { get; } = new Word(
        "นาที",
        new Transcription(paiboon: "naa-thii", ipa: "naː.tʰiː"),
        ["minute"]);

    /// <summary>Hour + ThaiNumber minute reading + "นาที", per romanization scheme.</summary>
    private sealed class MinutesTranscription(ITranscription? hour, ThaiNumber? minute, ITranscription? natee) : ITranscription
    {
        public string? Paiboon => Compose(hour?.Paiboon, minute?.Paiboon, natee?.Paiboon);
        public string? IPA => Compose(hour?.IPA, minute?.IPA, natee?.IPA);
        public string Full => Compose(hour?.Full, minute?.Paiboon, natee?.Full);

        private static string Compose(params string?[] parts) =>
            string.Join(" ", parts.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    private ThaiClockPanelVm BuildHourPanel(int hour, int minute)
    {
        var hourWord = _hourByOrdinal.TryGetValue(hour, out var h) ? h.Word : null;
        string hourThai = hourWord?.Value ?? string.Empty;
        string thai = hourThai;
        string audible = hourThai;
        ITranscription? transcription = hourWord?.Transcription;

        // no "ศูนย์ นาที" part for :00
        if (minute > 0)
        {
            var minuteTn = _minutes[Math.Clamp(minute, 0, 60)];
            thai = string.Join(" ", new[] { hourThai, minuteTn.Name, NateeWord.Value }.Where(s => s.Length > 0));
            audible = string.Join(" ", new[] { hourThai, minuteTn.Audible, NateeWord.Value }.Where(s => s.Length > 0));
            transcription = new MinutesTranscription(hourWord?.Transcription, minuteTn, NateeWord.Transcription);
        }

        return new ThaiClockPanelVm(thai, transcription, new ClockAudible(audible));
    }

    private static ThaiClockPanelVm CenterPanel { get; } = new(
        "ตอนนี้กี่โมงแล้ว",
        new Transcription(
            paiboon: "dtɔ̄-nii gìi moong lɛ́ɛo",
            ipa: "tɔ̄.nīː kìː mōːŋ lɛ̂ːw"),
        new ClockAudible("ตอนนี้กี่โมงแล้ว"));

    public void Receive(RomanizationOptionChanged message) => RomanizationOption = message.Value;

    [RelayCommand]
    private void SelectTimeOfDay(int ordinal)
    {
        var tod = _timeOfDayByOrdinal[ordinal];
        SelectedOuterOrdinal = ordinal;
        SelectedInnerHour = -1;
        SelectedDayNightSegment = -1;
        IsCenterSelected = false;
        SelectedPanel = new ThaiClockPanelVm(
            tod.Word?.Value ?? string.Empty,
            tod.Word?.Transcription,
            tod);
    }

    [RelayCommand]
    private void SelectHour(int hour)
    {
        SelectedInnerHour = hour;
        SelectedOuterOrdinal = -1;
        SelectedDayNightSegment = -1;
        IsCenterSelected = false;
        var now = DateTime.Now;
        _lastMinute = now.Minute;
        CurrentTimeText = $"{hour:00}:{now.Minute:00}";
        SelectedPanel = BuildHourPanel(hour, now.Minute);
    }

    /// <summary>Default selection when the view opens: the current hour + live minutes.</summary>
    [RelayCommand]
    private void SelectCurrentHour() => SelectHour(DateTime.Now.Hour);

    [RelayCommand]
    private void SelectDayNight(int segment)
    {
        SelectedDayNightSegment = segment;
        SelectedOuterOrdinal = -1;
        SelectedInnerHour = -1;
        IsCenterSelected = false;
        SelectedPanel = segment switch
        {
            0 => new ThaiClockPanelVm("กลางวัน", null, new ClockAudible("กลางวัน")),
            1 => new ThaiClockPanelVm("กลางคืน", null, new ClockAudible("กลางคืน")),
            _ => null,
        };
    }

    [RelayCommand]
    private void SelectCenter()
    {
        SelectedOuterOrdinal = -1;
        SelectedInnerHour = -1;
        SelectedDayNightSegment = -1;
        IsCenterSelected = true;
        SelectedPanel = CenterPanel;
        var now = DateTime.Now;
        CurrentTimeText = now.ToString("HH:mm");
        _lastMinute = now.Minute;
    }
}
