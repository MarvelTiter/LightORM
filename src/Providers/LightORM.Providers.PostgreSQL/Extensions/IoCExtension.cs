using LightORM.Interfaces;
using LightORM.Models;

namespace LightORM.Providers.PostgreSQL.Extensions;

public static class IoCExtension
{
    extension(IExpressionContextSetup options)
    {
        public void UsePostgreSQL(string masterConnectString, params string[] slaveConnectStrings)
        => options.UsePostgreSQL("MainDb", masterConnectString, slaveConnectStrings);
        public void UsePostgreSQL(string? key, string masterConnectString, params string[] slaveConnectStrings)
        {
            //var provider = PostgreSQLProvider.Create(masterConnectString, slaveConnectStrings);
            //options.SetDatabase(key, DbBaseType.PostgreSQL, provider);
            UsePostgreSQL(options, set =>
            {
                set.DbKey = key;
                set.MasterConnectionString = masterConnectString;
                set.SalveConnectionStrings = slaveConnectStrings;
            });
        }
        public void UsePostgreSQL(Action<IDbOption> setting)
        {
            var dbOption = new DataBaseOption<PostgreSQLTableOptions>();
            setting.Invoke(dbOption);
            if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
            {
                throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
            }
            var provider = PostgreSQLProvider.Create(dbOption);
            options.SetDatabase(dbOption.DbKey ?? "MainDb", DbBaseType.PostgreSQL, provider);
        }
    }

    extension(IDbOption option)
    {
        public void ConfigureOracle(Action<PostgreSQLTableOptions> config)
        {
            config.Invoke(option.GetOption<PostgreSQLTableOptions>());
        }
    }
}
