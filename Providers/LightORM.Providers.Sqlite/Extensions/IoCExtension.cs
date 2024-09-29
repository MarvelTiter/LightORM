using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.Sqlite.Extensions;

public static class IoCExtension
{
    public static void UseSqlite(this ExpressionSqlOptions options, string masterConnectString, params string[] slaveConnectStrings)
        => UseSqlite(options, "MainDb", masterConnectString, slaveConnectStrings);
    public static void UseSqlite(this ExpressionSqlOptions options, string? key, string masterConnectString, params string[] slaveConnectStrings)
    {
        options.AddDatabaseCustomer(DbBaseType.Sqlite, new CustomSqlite());
        options.AddDatabaseHandler(DbBaseType.Sqlite, o => new SqliteTableHandler(o));
        var provider = new SqliteProvider(masterConnectString, SQLiteFactory.Instance, slaveConnectStrings);
        options.SetDatabase(key, DbBaseType.Sqlite, provider);
    }
}
