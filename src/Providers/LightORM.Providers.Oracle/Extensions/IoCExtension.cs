using LightORM.Interfaces;
using LightORM.Models;
using Oracle.ManagedDataAccess.Client;
using LightORM.Providers.Oracle;
namespace LightORM.Providers.Oracle.Extensions;

public static class IoCExtension
{
    extension(IExpressionContextSetup options)
    {
        public void UseOracle(string masterConnectString, params string[] slaveConnectStrings)
        => options.UseOracle("MainDb", masterConnectString, slaveConnectStrings);
        public void UseOracle(string? key, string masterConnectString, params string[] slaveConnectStrings)
        {
            //var provider = OracleProvider.Create(masterConnectString, slaveConnectStrings);
            //options.SetDatabase(key, DbBaseType.Oracle, provider);
            UseOracle(options, set =>
            {
                set.DbKey = key;
                set.MasterConnectionString = masterConnectString;
                set.SalveConnectionStrings = slaveConnectStrings;
            });
        }
        public void UseOracle(Action<IDbOption> setting)
        {
            var dbOption = new DataBaseOption<OracleTableOptions>();
            setting.Invoke(dbOption);
            if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
            {
                throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
            }
            var provider = OracleProvider.Create(dbOption);
            options.SetDatabase(dbOption.DbKey ?? "MainDb", DbBaseType.Oracle, provider);
        }
    }

    extension(IDbOption option)
    {
        public void ConfigureOracle(Action<OracleTableOptions> config)
        {
            config.Invoke(option.GetOption<OracleTableOptions>());
        }
    }
}
