using System.Data;

namespace ThaiReference.Utilities.Extensions;

public static class DataRowExtension
{
    public static T GetValueOrDefault<T>(this DataRow row, string columnName) where T : struct
    {
        if (row.IsNull(columnName)) return default;
        else return (T)Convert.ChangeType(row[columnName], typeof(T));
    }

    public static bool GetValueOrDefault<T>(this DataRow row, string columnName, out T outvar) where T : struct
    {
        var isNull = row.IsNull(columnName);
        outvar = isNull ? default : (T)Convert.ChangeType(row[columnName], typeof(T));
        return !isNull;
    }

    public static T GetValueOrNull<T>(this DataRow row, string columnName) where T : class
    {
        if (row.IsNull(columnName) || typeof(T) == typeof(string) && string.IsNullOrEmpty(row.Field<string>(columnName))) return null!;
        else return row.Field<T>(columnName)!;
    }

    public static bool GetValueOrNull<T>(this DataRow row, string columnName, out T outvar) where T : class
    {
        var isNull = row.IsNull(columnName) || typeof(T) == typeof(string) && string.IsNullOrEmpty(row.Field<string>(columnName));
        outvar = isNull ? null! : row.Field<T>(columnName)!;
        return !isNull;
    }
}
