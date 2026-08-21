using LightORM.Interfaces;
using LightORM.Models;
using System.Data.SQLite;

namespace LightORM.Providers.Sqlite.Extensions;

public static class IoCExtension
{
    extension(IExpressionContextSetup options)
    {
        public void UseSqlite(string masterConnectString, params string[] slaveConnectStrings)
        => UseSqlite(options, "MainDb", masterConnectString, slaveConnectStrings);
        public void UseSqlite(string? key, string masterConnectString, params string[] slaveConnectStrings)
        {
            UseSqlite(options, set =>
            {
                set.DbKey = key;
                set.MasterConnectionString = masterConnectString;
                set.SalveConnectionStrings = slaveConnectStrings;
            });
        }
        public void UseSqlite(Action<IDbOption> setting)
        {
            var dbOption = new DataBaseOption<SqliteTableOptions>();
            setting.Invoke(dbOption);
            if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
            {
                throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
            }
            var provider = SqliteProvider.Create(dbOption);
            options.SetDatabase(dbOption.DbKey ?? "MainDb", DbBaseType.Sqlite, provider);
        }
    }

    extension(IDbOption option)
    {
        public void ConfiguraSqlite(Action<SqliteTableOptions> config)
        {
            config.Invoke(option.GetOption<SqliteTableOptions>());
        }
    }
}
