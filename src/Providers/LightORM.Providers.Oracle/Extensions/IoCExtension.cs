using LightORM.Interfaces;
using LightORM.Models;
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

        /// <summary>
        /// 只登记"从配置创建 Oracle provider"的工厂，不注册任何实例。
        /// <para>配合 <c>AddLightOrm(..., IConfiguration)</c> 使用：配置里 <c>Type = "Oracle"</c> 的条目才会被创建。</para>
        /// </summary>
        public void UseOracle()
        {
            options.RegisterDatabase(DbBaseType.Oracle, database => OracleProvider.Create(database));
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
