using LiteDB;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.DataAccess;

public static class ApiMigrate
{
    private const string ERROR_MSG = "Error loading {0} to LiteDB database from API: {1}";

    private static IEnumerable<T> ReadAllPages<T>(Func<int, int, IEnumerable<T>> readPage, int pageSize = 200)
    {
        int page = 1;
        while (true)
        {
            List<T> items = [.. readPage(page, pageSize)];
            if (items.Count == 0)
                yield break;

            foreach (var item in items)
                yield return item;

            if (items.Count < pageSize)
                yield break;

            page++;
        }
    }

    private static void Load<T>(LiteDatabase litedb, IEnumerable<T> items, string collectionName)
    {
        var coll = litedb.GetCollection<T>(collectionName);
        try
        {
            foreach (var item in items)
                coll.Insert(item);
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format(ERROR_MSG, collectionName, ex.Message));
        }
    }

    private static void Populate(LiteDatabase db, ApiDataService api)
    {
        Load<IConsonant>(db, api.Consonants, "consonants");
        Load<IVowel>(db, api.Vowels, "vowels");
        Load<IMark>(db, api.Marks, "marks");
        Load<IConsonantClass>(db, api.Classes, "classes");
        Load<INumeral>(db, api.Numerals, "numerals");
        Load<IPronoun>(db, api.Pronouns, "pronouns");
        Load<IGridPositionCell>(db, api.GridPositionCells, "gridpositioncells");
        Load<IPosition>(db, api.Positions, "positions");
        Load<IWeekday>(db, api.Weekdays, "weekdays");
        Load<ICelestialBody>(db, api.CelestialBodies, "celestialbodies");
        Load<ISeason>(db, api.Seasons, "seasons");
        Load<ITimeOfDay>(db, api.TimeOfDayItems, "timeofdayitems");
        Load<IHour>(db, api.HourItems, "houritems");
        Load<ICluster>(db, api.Clusters, "clusters");
        Load<IHomophoneGroup>(db, api.Homophones, "homophones");
        Load<IMinimalSet>(db, ReadAllPages((page, size) => api.MinimalSets(page: page, pageSize: size).Items), "minimalsets");
        Load<IDirection>(db, api.Directions, "directions");
        Load<IHueFamily>(db, api.HueFamilies, "huefamilies");
    }

    private static void WriteToTempThenReplace(string targetPath, Action<LiteDatabase> write)
    {
        var directory = Path.GetDirectoryName(targetPath) ?? ".";
        Directory.CreateDirectory(directory);
        var tempPath = Path.Combine(directory, $".{Path.GetFileName(targetPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            using (var db = new LiteDatabase(tempPath))
                write(db);
            File.Move(tempPath, targetPath, overwrite: true);
        }
        catch
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            throw;
        }
    }

    internal static Task Do(LiteDBManager litedbm, ApiDataService api)
    {
        WriteToTempThenReplace(litedbm.DbPath, db => Populate(db, api));
        return Task.CompletedTask;
    }

    public static Task Do(string outputPath, ApiDataService api)
    {
        WriteToTempThenReplace(outputPath, db => Populate(db, api));
        return Task.CompletedTask;
    }
}
