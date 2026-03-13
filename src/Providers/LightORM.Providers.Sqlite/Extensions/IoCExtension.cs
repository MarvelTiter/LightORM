using LightORM.Interfaces;
using LightORM.Models;
using System.Data.SQLite;

namespace LightORM.Providers.Sqlite.Extensions;

public static class IoCExtension
{
    public static void UseSqlite(this IExpressionContextSetup options, string masterConnectString, params string[] slaveConnectStrings)
        => UseSqlite(options, "MainDb", masterConnectString, slaveConnectStrings);
    public static void UseSqlite(this IExpressionContextSetup options, string? key, string masterConnectString, params string[] slaveConnectStrings)
    {
        //var provider = SqliteProvider.Create(masterConnectString, slaveConnectStrings);
        //options.SetDatabase(key, DbBaseType.Sqlite, provider);
        UseSqlite(options, set =>
        {
            set.DbKey = key;
            set.MasterConnectionString = masterConnectString;
            set.SalveConnectionStrings = slaveConnectStrings;
        });
    }
    public static void UseSqlite(this IExpressionContextSetup options, Action<IDbOption> setting)
    {
        var dbOption = new DataBaseOption(new SqliteMethodResolver());
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        var provider = SqliteProvider.Create(dbOption);
        options.SetDatabase(dbOption.DbKey ?? "MainDb", DbBaseType.Sqlite, provider);
    }
}
