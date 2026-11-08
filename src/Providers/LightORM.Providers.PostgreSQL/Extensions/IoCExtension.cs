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

        /// <summary>
        /// 只登记"从配置创建 PostgreSQL provider"的工厂，不注册任何实例。
        /// <para>配合容器里的 <c>IConfiguration</c>（配置节 <c>LightOrmDatabases</c>）使用：配置里 <c>Type = "PostgreSQL"</c> 的条目才会被创建。</para>
        /// </summary>
        public void UsePostgreSQL()
        {
            options.RegisterDatabase(DbBaseType.PostgreSQL, database => PostgreSQLProvider.Create(database));
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
