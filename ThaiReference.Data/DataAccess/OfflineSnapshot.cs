using LiteDB;

namespace ThaiReference.DataAccess;

/// <summary>Helpers for the packaged / AppData offline LiteDB snapshot.</summary>
public static class OfflineSnapshot
{
    public const long MinBytes = 64 * 1024;

    static readonly string[] RequiredCollections =
    [
        "directions",
        "positions",
        "gridpositioncells",
        "consonants",
    ];

    /// <summary>
    /// True when the file looks like a usable offline snapshot (not empty / pre-Space schema).
    /// </summary>
    public static bool HasRequiredCollections(string dbPath)
    {
        if (!File.Exists(dbPath) || new FileInfo(dbPath).Length < MinBytes)
            return false;

        try
        {
            using var db = new LiteDatabase($"Filename={dbPath};ReadOnly=true");
            foreach (var name in RequiredCollections)
            {
                if (db.GetCollection(name).Count() <= 0)
                    return false;
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
