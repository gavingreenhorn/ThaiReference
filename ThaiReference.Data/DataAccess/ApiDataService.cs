using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using ThaiReference.Models;
using ThaiReference.Models.BaseModels;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.DataAccess;

public class ApiDataService : IDataService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _http;
    private readonly string? _snapshotDbDir;
    private readonly Func<string, Task>? _ensureSnapshotDb;
    private readonly ApiAvailabilityMonitor? _availability;
    private LiteDataService? _fallback;
    private readonly Lock _fallbackLock = new();

    public ApiDataService(
        IConfiguration config,
        string snapshotDbDir,
        Func<string, Task>? ensureSnapshotDb = null,
        ApiAvailabilityMonitor? availability = null)
        : this(
            new HttpClient
            {
                BaseAddress = new Uri(GetBaseUrl(config)),
                Timeout = TimeSpan.FromSeconds(1.5),
            },
            snapshotDbDir,
            ensureSnapshotDb,
            availability)
    {
    }

    public static string GetBaseUrl(IConfiguration config)
    {
        var host = config["ApiHost"] ?? "127.0.0.1";
        var port = config["ApiPort"] ?? "5000";
        return $"http://{host}:{port}";
    }

    /// <summary>HTTP-only construction for tools (e.g. FetchLiteDb). Fallback is unavailable.</summary>
    public ApiDataService(string baseUrl)
        : this(
            new HttpClient { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromSeconds(1.5) },
            snapshotDbDir: null,
            ensureSnapshotDb: null,
            availability: null)
    {
    }

    public ApiDataService(
        string baseUrl,
        string snapshotDbDir,
        Func<string, Task>? ensureSnapshotDb = null,
        ApiAvailabilityMonitor? availability = null)
        : this(
            new HttpClient { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromSeconds(1.5) },
            snapshotDbDir,
            ensureSnapshotDb,
            availability)
    {
    }

    public ApiDataService(
        HttpClient http,
        string? snapshotDbDir,
        Func<string, Task>? ensureSnapshotDb = null,
        ApiAvailabilityMonitor? availability = null)
    {
        _http = http;
        _snapshotDbDir = snapshotDbDir;
        _ensureSnapshotDb = ensureSnapshotDb;
        _availability = availability;
        if (_availability is not null)
            _availability.Changed += up =>
            {
                if (up)
                    ClearCollectionCaches();
            };
    }

    public string? DbName { get; set; }
    public bool IsInitialized => true;
    public Task InitializeAsync() => Task.CompletedTask;

    private T Fetch<T>(string path)
    {
        var response = _http.GetAsync(path).ConfigureAwait(false).GetAwaiter().GetResult();
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"GET {path} failed with {(int)response.StatusCode} ({response.StatusCode}).");
        }

        return response.Content
            .ReadFromJsonAsync<T>(JsonOptions)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult()!;
    }

    private List<T> FetchList<T>(string path) => Fetch<List<T>>(path) ?? [];

    private LiteDataService FallbackDB
    {
        get
        {
            if (_fallback != null) return _fallback;
            lock (_fallbackLock)
            {
                if (_fallback != null) return _fallback;
                if (string.IsNullOrEmpty(_snapshotDbDir))
                    throw new InvalidOperationException("Snapshot DB directory is not configured for fallback.");
                if (string.IsNullOrEmpty(DbName))
                    throw new InvalidOperationException("DbName is required for fallback.");
                _fallback = new LiteDataService(_snapshotDbDir, _ensureSnapshotDb) { DbName = DbName };
                _fallback.InitializeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                return _fallback;
            }
        }
    }

    private IEnumerable<T> GetOrFallback<T>(
        Func<List<T>> fetchFromApi,
        Func<LiteDataService, IEnumerable<T>> fromFallback)
    {
        if (_availability is { IsUp: false })
            return fromFallback(FallbackDB).ToList();

        return LocalFallbackHandler.GetList(
            fetchFromApi,
            () =>
            {
                _availability?.MarkDown();
                return fromFallback(FallbackDB);
            },
            preferFallbackWhenApiEmpty: true);
    }

    private T GetOrFallback<T>(
        Func<T> fetchFromApi,
        Func<LiteDataService, T> fromFallback)
    {
        if (_availability is { IsUp: false })
            return fromFallback(FallbackDB);

        return LocalFallbackHandler.GetValue(
            fetchFromApi,
            () =>
            {
                _availability?.MarkDown();
                return fromFallback(FallbackDB);
            });
    }

    public void ClearCollectionCaches()
    {
        _sounds = null;
        _consonants = null;
        _vowels = null;
        _marks = null;
        _classes = null;
        _numerals = null;
        _pronouns = null;
        _gridPositionCells = null;
        _positions = null;
        _weekdays = null;
        _celestialBodies = null;
        _months = null;
        _timeOfDayItems = null;
        _hourItems = null;
        _clusters = null;
        _homophones = null;
        _minimalSets = null;
        _directions = null;
        _hueFamilies = null;
    }

    // ------------------------------------------------------------------
    // IDataService collections (cached on first access; fallback to embedded LiteDB on API failure
    // ------------------------------------------------------------------

    private List<ISound>? _sounds;
    private List<IConsonant>? _consonants;
    private List<IVowel>? _vowels;
    private List<IMark>? _marks;
    private List<IConsonantClass>? _classes;
    private List<INumeral>? _numerals;
    private List<IPronoun>? _pronouns;
    private List<IGridPositionCell>? _gridPositionCells;
    private List<IPosition>? _positions;
    private List<IWeekday>? _weekdays;
    private List<ICelestialBody>? _celestialBodies;
    private List<IMonth>? _months;
    private List<ITimeOfDay>? _timeOfDayItems;
    private List<IHour>? _hourItems;
    private List<ICluster>? _clusters;
    private List<IHomophoneGroup>? _homophones;
    private List<IMinimalSet>? _minimalSets;
    private List<IDirection>? _directions;
    private List<IHueFamily>? _hueFamilies;

    public IEnumerable<ISound> Sounds => _sounds ??= GetOrFallback(
        () => FetchList<SoundDto>("/thai/sounds").Select(s => (ISound)s.ToSound()).ToList(),
        fb => fb.Sounds).ToList();

    public IEnumerable<IConsonant> Consonants => _consonants ??= GetOrFallback(
        () => FetchList<ConsonantDto>("/thai/consonants").Select(c => (IConsonant)c.ToConsonant()).ToList(),
        fb => fb.Consonants).ToList();

    public IEnumerable<IVowel> Vowels => _vowels ??= GetOrFallback(
        () => FetchList<VowelDto>("/thai/vowels").Select(v => (IVowel)v.ToVowel()).ToList(),
        fb => fb.Vowels).ToList();

    public IEnumerable<IMark> Marks => _marks ??= GetOrFallback(
        () => FetchList<MarkDto>("/thai/marks").Select(m => (IMark)m.ToMark()).ToList(),
        fb => fb.Marks).ToList();

    public IEnumerable<IConsonantClass> Classes => _classes ??= GetOrFallback(
        () => FetchList<ThaiClassDto>("/thai/classes").Select(c => (IConsonantClass)c.ToConsonantClass()).ToList(),
        fb => fb.Classes).ToList();

    public IEnumerable<INumeral> Numerals => _numerals ??= GetOrFallback(
        () => FetchList<NumeralDto>("/thai/numerals").Select(n => (INumeral)n.ToNumeral()).ToList(),
        fb => fb.Numerals).ToList();

    public IEnumerable<IPronoun> Pronouns => _pronouns ??= GetOrFallback(
        () => FetchList<PronounDto>("/thai/pronouns").Select(p => (IPronoun)p.ToPronoun()).ToList(),
        fb => fb.Pronouns).ToList();

    public IEnumerable<IGridPositionCell> GridPositionCells => _gridPositionCells ??= GetOrFallback(
        () => FetchList<GridPositionCellDto>("/thai/grid-positions").Select<GridPositionCellDto, IGridPositionCell>(d => d.ToGridPositionCell()).ToList(),
        fb => fb.GridPositionCells).ToList();

    public IEnumerable<IPosition> Positions => _positions ??= GetOrFallback(
        () => FetchList<PositionDto>("/thai/positions").Select<PositionDto, IPosition>(d => d.ToPosition()).ToList(),
        fb => fb.Positions).ToList();

    public IEnumerable<IWeekday> Weekdays => _weekdays ??= GetOrFallback(
        () => FetchList<WeekdayDto>("/thai/weekdays").Select<WeekdayDto, IWeekday>(d => d.ToWeekday()).ToList(),
        fb => fb.Weekdays).ToList();

    public IEnumerable<ICelestialBody> CelestialBodies => _celestialBodies ??= GetOrFallback(
        () => FetchList<CelestialBodyDto>("/thai/celestial-bodies").Select<CelestialBodyDto, ICelestialBody>(d => d.ToCelestialBody()).ToList(),
        fb => fb.CelestialBodies).ToList();

    public IEnumerable<IMonth> Months => _months ??= GetOrFallback(
        () => FetchList<MonthDto>("/thai/months").Select<MonthDto, IMonth>(d => d.ToMonth()).ToList(),
        fb => fb.Months).ToList();

    public IEnumerable<ITimeOfDay> TimeOfDayItems => _timeOfDayItems ??= GetOrFallback(
        () => FetchList<TimeOfDayDto>("/thai/time-of-day").Select<TimeOfDayDto, ITimeOfDay>(d => d.ToTimeOfDay()).ToList(),
        fb => fb.TimeOfDayItems).ToList();

    public IEnumerable<IHour> HourItems => _hourItems ??= GetOrFallback(
        () => FetchList<HourDto>("/thai/hours").Select<HourDto, IHour>(d => d.ToHour()).ToList(),
        fb => fb.HourItems).ToList();

    public IEnumerable<ICluster> Clusters => _clusters ??= GetOrFallback(
        () => FetchList<ClusterDto>("/thai/clusters").Select(c => (ICluster)c.ToCluster()).ToList(),
        fb => fb.Clusters).ToList();

    public IEnumerable<IHomophoneGroup> Homophones => _homophones ??= GetOrFallback(
        () => FetchList<HomophoneGroupDto>("/thai/homophones").Select(g => (IHomophoneGroup)g.ToHomophoneGroup()).ToList(),
        fb => fb.Homophones).ToList();

    public IEnumerable<IMinimalSet> MinimalSets => _minimalSets ??= GetOrFallback(
        () => FetchList<MinimalSetDto>("/thai/minimal-sets").Select(m => (IMinimalSet)m.ToMinimalSet()).ToList(),
        fb => fb.MinimalSets).ToList();

    public IEnumerable<IDirection> Directions => _directions ??= GetOrFallback(
        () => FetchList<DirectionDto>("/thai/directions").Select<DirectionDto, IDirection>(d => d.ToDirection()).ToList(),
        fb => fb.Directions).ToList();

    public IEnumerable<IHueFamily> HueFamilies => _hueFamilies ??= GetOrFallback(
        () => FetchList<HueFamilyDto>("/thai/colors").Select<HueFamilyDto, IHueFamily>(h => h.ToHueFamily()).ToList(),
        fb => fb.HueFamilies).ToList();

    // ------------------------------------------------------------------
    // DTOs
    // ------------------------------------------------------------------

    private record TranscriptionDto(string? Paiboon, string? Ipa)
    {
        public Transcription ToTranscription() => new()
        {
            Paiboon = Paiboon,
            IPA = Ipa,
        };
    }

    private record SoundDto(
        int Id,
        string Value,
        string SoundType,
        bool IsConsonantSound,
        bool IsInitialOnly,
        string? MediaUrl)
    {
        public Sound ToSound() => new()
        {
            Value = Value,
            SoundType = SoundType,
            IsConsonantSound = IsConsonantSound,
            IsInitialOnly = IsInitialOnly,
            MediaUrl = MediaUrl,
            Media = [],
        };
    }

    private record WordDto(
        int Id,
        string Value,
        TranscriptionDto Transcription,
        List<string> Lexemes)
    {
        public Word ToWord() => new()
        {
            Value = Value,
            Transcription = Transcription.ToTranscription(),
            Lexemes = Lexemes,
        };
    }

    private record VowelGraphemeDto(string Value, bool IsClosed)
    {
        public VowelGrapheme ToVowelGrapheme() => new()
        {
            Value = Value,
            IsClosed = IsClosed,
        };
    }

    private record ConsonantDto(
        string Grapheme,
        string Phoneme,
        string ConsonantClass,
        string Ending,
        TranscriptionDto Transcription,
        SoundDto InitialSound,
        SoundDto? EndingSound,
        List<WordDto> SampleWords)
    {
        public Consonant ToConsonant() => new()
        {
            Grapheme = Grapheme,
            Phoneme = Phoneme,
            Class = ConsonantClass,
            Ending = Ending,
            Transcription = Transcription.ToTranscription(),
            InitialSound = InitialSound.ToSound(),
            EndingSound = EndingSound?.ToSound(),
            SampleWords = SampleWords.Select(w => (IWord)w.ToWord()).ToList(),
        };
    }

    private record VowelDto(
        bool IsDiphtong,
        SoundDto Sound,
        List<VowelGraphemeDto> Graphemes,
        List<WordDto> SampleWords,
        string? MediaUrl)
    {
        public Vowel ToVowel() => new()
        {
            IsDiphtong = IsDiphtong,
            Sound = Sound.ToSound(),
            Graphemes = Graphemes.Select(g => (IVowelGrapheme)g.ToVowelGrapheme()),
            SampleWords = SampleWords.Select(w => (IWord)w.ToWord()).ToList(),
            MediaUrl = MediaUrl,
        };
    }

    private record MarkDto(
        string Grapheme,
        bool ToneMark,
        List<WordDto> Words,
        string? EnglishLabel)
    {
        public Mark ToMark() => new()
        {
            Grapheme = Grapheme,
            ToneMark = ToneMark,
            Media = null,
            Words = Words.Select(w => (IWord)w.ToWord()).ToList(),
            EnglishLabel = EnglishLabel,
        };
    }

    private record ThaiClassDto(
        string Code,
        string? EnglishLabel,
        List<WordDto> Words)
    {
        public ConsonantClass ToConsonantClass() => new()
        {
            Code = Code,
            EnglishLabel = EnglishLabel,
            Media = null,
            Words = Words.Select(w => (IWord)w.ToWord()).ToList(),
        };
    }

    private record NumeralDto(string Grapheme, WordDto Name)
    {
        public Numeral ToNumeral() => new()
        {
            Grapheme = Grapheme,
            Name = Name.ToWord(),
        };
    }

    private record PronounDto(
        WordDto Name,
        int Person,
        int Plurality,
        string Formality,
        string Gender,
        List<string> Types)
    {
        public Pronoun ToPronoun() => new()
        {
            Name = Name.ToWord(),
            Person = Person,
            Plurality = Plurality,
            Formality = Formality,
            Gender = Gender,
            Types = Types,
        };
    }

    private record GridPositionCellDto(
        int Ordinal,
        int Row,
        int Col,
        string? TileStyle,
        string? Label,
        List<WordDto> Words)
    {
        public GridPositionCell ToGridPositionCell() => new()
        {
            Ordinal = Ordinal,
            Row = Row,
            Col = Col,
            TileStyle = TileStyle,
            Label = Label,
            Words = Words.Select(w => w.ToWord()).ToList(),
        };
    }

    private record SentenceDto(int Id, string Translation, List<WordDto> Words)
    {
        public Sentence ToSentence() => new()
        {
            Id = Id,
            Translation = Translation,
            Words = Words.Select(w => w.ToWord()).ToList(),
        };
    }

    private record PositionDto(int Id, string Lexeme, List<WordDto> Words, SentenceDto Example)
    {
        public Position ToPosition()
        {
            var words = Words.Select(w => w.ToWord()).ToList();
            return new Position
            {
                Id = Id,
                Lexeme = Lexeme,
                Words = words,
                Word = MergeCompositeWord(words, Lexeme),
                Example = Example.ToSentence(),
            };
        }
    }

    private static Word MergeCompositeWord(List<Word> parts, string lexeme)
    {
        if (parts.Count == 1)
        {
            var only = parts[0];
            return new Word(only.Value, only.Transcription, [lexeme]);
        }

        string value = string.Concat(parts.Select(p => p.Value));
        string Join(Func<ITranscription, string?> pick) =>
            string.Join(" ", parts
                .Select(p => pick(p.Transcription))
                .Where(s => !string.IsNullOrWhiteSpace(s)));

        var transcription = new Transcription(
            Join(t => t.Paiboon),
            Join(t => t.IPA));
        return new Word(value, transcription, [lexeme]);
    }

    private record WeekdayDto(
        int Ordinal,
        WordDto Word,
        string? Deity,
        [property: JsonPropertyName("image_url")] string? ImageUrl)
    {
        public Weekday ToWeekday() => new()
        {
            Ordinal = Ordinal,
            Word = Word.ToWord(),
            Deity = Deity,
            ImageUrl = ImageUrl,
        };
    }

    private record CelestialBodyDto(
        int Ordinal,
        string Glyph,
        [property: JsonPropertyName("color_hex")] string ColorHex,
        WordDto Word)
    {
        public CelestialBody ToCelestialBody() => new()
        {
            Ordinal = Ordinal,
            Glyph = Glyph,
            ColorHex = ColorHex,
            Word = Word.ToWord(),
        };
    }

    private record TimeOfDayDto(int Ordinal, WordDto Word)
    {
        public TimeOfDay ToTimeOfDay() => new()
        {
            Ordinal = Ordinal,
            Word = Word.ToWord(),
        };
    }

    private record HourDto(int Ordinal, WordDto Word)
    {
        public Hour ToHour() => new()
        {
            Ordinal = Ordinal,
            Word = Word.ToWord(),
        };
    }

    private record ZodiacDto(
        int Ordinal,
        string EnglishName,
        string SanskritName,
        string SanskritLexeme,
        string Element,
        string? ImageUrl)
    {
        public Zodiac ToZodiac() => new()
        {
            Ordinal = Ordinal,
            EnglishName = EnglishName,
            SanskritName = SanskritName,
            SanskritLexeme = SanskritLexeme,
            Element = Element,
            ImageUrl = ImageUrl,
        };
    }

    private record MonthDto(
        int Ordinal,
        WordDto Word,
        string ShortName,
        string EnglishName,
        ZodiacDto Zodiac)
    {
        public Month ToMonth() => new()
        {
            Ordinal = Ordinal,
            Word = Word.ToWord(),
            ShortName = ShortName,
            EnglishName = EnglishName,
            Zodiac = Zodiac.ToZodiac(),
        };
    }

    private record DirectionDto(
        int Id,
        string Label,
        string Lexeme,
        string Spelling,
        TranscriptionDto Transcription)
    {
        public Direction ToDirection() => new()
        {
            Id = Id,
            Label = Label,
            Lexeme = Lexeme,
            Spelling = Spelling,
            Transcription = Transcription.ToTranscription(),
        };
    }

    private record ClusterDto(
        int Id,
        string Type,
        string? Subtype,
        string Value,
        List<WordDto> SampleWords)
    {
        public Cluster ToCluster() => new()
        {
            Type = Type,
            Subtype = Subtype,
            Value = Value,
            SampleWords = SampleWords.Select(w => (IWord)w.ToWord()).ToList(),
        };
    }

    private record HomophoneGroupDto(
        TranscriptionDto Transcription,
        List<WordDto> Words)
    {
        public HomophoneGroup ToHomophoneGroup() => new()
        {
            Transcription = Transcription.ToTranscription(),
            Words = Words.Select(w => (IWord)w.ToWord()).ToList(),
        };
    }

    private record MinimalSetDto(
        int Id,
        string Label,
        List<WordDto> Words)
    {
        public MinimalSet ToMinimalSet() => new()
        {
            Label = Label,
            Words = Words.Select(w => (IWord)w.ToWord()).ToList(),
        };
    }

    private record ColorDto(
        int Id,
        string Hex,
        string EnglishLabel,
        List<WordDto> Words,
        int LuminanceOrder)
    {
        public ThaiColor ToColor() => new()
        {
            Id = Id,
            Hex = Hex,
            EnglishLabel = EnglishLabel,
            LuminanceOrder = LuminanceOrder,
            Words = Words.Select(w => (IWord)w.ToWord()).ToList(),
        };
    }

    private record HueFamilyDto(
        int Id,
        string ThaiName,
        string EnglishName,
        string Hex,
        int DisplayOrder,
        List<ColorDto> Colors)
    {
        public HueFamily ToHueFamily() => new()
        {
            Id = Id,
            ThaiName = ThaiName,
            EnglishName = EnglishName,
            Hex = Hex,
            DisplayOrder = DisplayOrder,
            Colors = Colors.Select(c => (IColor)c.ToColor()).ToList(),
        };
    }
}
