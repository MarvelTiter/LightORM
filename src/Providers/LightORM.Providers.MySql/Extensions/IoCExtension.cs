using LightORM.Interfaces;
using LightORM.Models;
namespace LightORM.Providers.MySql.Extensions;

public static class IoCExtension
{
    extension(IExpressionContextSetup options)
    {
        public void UseMySql(string masterConnectString, params string[] slaveConnectStrings)
        => options.UseMySql("MainDb", masterConnectString, slaveConnectStrings);
        public void UseMySql(string? key, string masterConnectString, params string[] slaveConnectStrings)
        {
            //var provider = MySqlProvider.Create(masterConnectString, slaveConnectStrings);
            //options.SetDatabase(key, DbBaseType.MySql, provider);
            UseMySql(options, set =>
            {
                set.DbKey = key;
                set.MasterConnectionString = masterConnectString;
                set.SalveConnectionStrings = slaveConnectStrings;
            });
        }
        public void UseMySql(Action<IDbOption> setting)
        {
            var dbOption = new DataBaseOption<MySqlTableOptions>();
            setting.Invoke(dbOption);
            if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
            {
                throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
            }
            var provider = MySqlProvider.Create(dbOption);
            options.SetDatabase(dbOption.DbKey ?? "MainDb", DbBaseType.MySql, provider);
        }
    }

    extension(IDbOption option)
    {
        public void ConfiguraMySql(Action<MySqlTableOptions> config) 
        {
            config.Invoke(option.GetOption<MySqlTableOptions>());
        }
    }
}
