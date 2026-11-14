using ThaiReference.Models.Interfaces;

namespace ThaiReference.DataAccess;

public interface IDataService
{
    string? DbName { get; set; }
    bool IsInitialized { get; }
    Task InitializeAsync();
    IEnumerable<ISound> Sounds { get; }
    IEnumerable<IConsonant> Consonants { get; }
    IEnumerable<IVowel> Vowels { get; }
    IEnumerable<IMark> Marks { get; }
    IEnumerable<IConsonantClass> Classes { get; }
    IEnumerable<INumeral> Numerals { get; }
    IEnumerable<IPronoun> Pronouns { get; }
    IEnumerable<IGridPositionCell> GridPositionCells { get; }
    IEnumerable<IPosition> Positions { get; }
    IEnumerable<IWeekday> Weekdays { get; }
    IEnumerable<ICelestialBody> CelestialBodies { get; }
    IEnumerable<ISeason> Seasons { get; }
    IEnumerable<ITimeOfDay> TimeOfDayItems { get; }
    IEnumerable<IHour> HourItems { get; }
    IEnumerable<ICluster> Clusters { get; }
    IEnumerable<IHomophoneGroup> Homophones { get; }
    MinimalSetPage MinimalSets(string? initialSoundLabel = null, int page = 1, int pageSize = MinimalSetPage.DefaultPageSize);
    IEnumerable<IDirection> Directions { get; }
    IEnumerable<IHueFamily> HueFamilies { get; }
}

public record MinimalSetPage(IReadOnlyList<IMinimalSet> Items, long TotalItems)
{
    public const int DefaultPageSize = 10;

    public static MinimalSetPage Empty { get; } = new([], 0);
}
