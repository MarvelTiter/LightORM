using LightORM.Abstracts;
using LightORM.Cache;
using LightORM.Context;
using LightORM.SqlMethodResolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Extension;

internal static class DbTypeExtensions
{
    private static readonly Dictionary<DbBaseType, string> prefix;
    private static readonly Dictionary<DbBaseType, string> emphasis;
    static DbTypeExtensions()
    {
        prefix = new()
        {
            [DbBaseType.SqlServer] = "@",
            [DbBaseType.SqlServer2012] = "@",
            [DbBaseType.MySql] = "?",
            [DbBaseType.Oracle] = ":",
            [DbBaseType.Sqlite] = "@",
        };

        emphasis = new()
        {
            [DbBaseType.SqlServer] = "[]",
            [DbBaseType.SqlServer2012] = "[]",
            [DbBaseType.MySql] = "``",
            [DbBaseType.Oracle] = "\"\"",
            [DbBaseType.Sqlite] = "``",
        };
    }

    public static string AttachPrefix(this DbBaseType dbBaseType, string name) => $"{prefix[dbBaseType]}{name}";
    public static string GetPrefix(this DbBaseType dbBaseType) => prefix[dbBaseType];

    public static string AttachEmphasis(this DbBaseType dbBaseType, string name) => emphasis[dbBaseType].Insert(1, name);
    public static string GetEmphasis(this DbBaseType dbBaseType) => emphasis[dbBaseType];




    public static SqlMethod GetSqlMethodResolver(this DbBaseType dbBaseType)
    {
        var cacheKey = $"SqlMethodResolver_{dbBaseType}";
        return dbBaseType switch
        {
            DbBaseType.Oracle => StaticCache<SqlMethod>.GetOrAdd(cacheKey, () => new OracleMethodResolver()),
            DbBaseType.MySql => StaticCache<SqlMethod>.GetOrAdd(cacheKey, () => new MySqlMethodResolver()),
            DbBaseType.Sqlite => StaticCache<SqlMethod>.GetOrAdd(cacheKey, () => new SqliteMethodResolver()),
            DbBaseType.SqlServer or DbBaseType.SqlServer2012 => StaticCache<SqlMethod>.GetOrAdd(cacheKey, () => new SqlServerMethodResolver()),
            _ => throw new NotSupportedException()
        };
    }
}
