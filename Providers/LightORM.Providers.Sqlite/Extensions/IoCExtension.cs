using System.Data.SQLite;

namespace LightORM.Providers.Sqlite.Extensions;

public static class IoCExtension
{
    private static CustomSqlite custom = new CustomSqlite();
    public static void UseSqlite(this ExpressionSqlOptions options, string masterConnectString, params string[] slaveConnectStrings)
        => UseSqlite(options, "MainDb", masterConnectString, slaveConnectStrings);
    public static void UseSqlite(this ExpressionSqlOptions options, string? key, string masterConnectString, params string[] slaveConnectStrings)
    {
        var provider = new SqliteProvider(custom
            , o => new SqliteTableHandler(o)
            , SQLiteFactory.Instance
            , masterConnectString
            , slaveConnectStrings);
        options.SetDatabase(key, DbBaseType.Sqlite, provider);
    }
}
