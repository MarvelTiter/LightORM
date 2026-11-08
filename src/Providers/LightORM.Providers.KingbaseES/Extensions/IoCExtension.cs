using LightORM.Interfaces;
using LightORM.Models;

namespace LightORM.Providers.KingbaseES.Extensions;

public static class IoCExtension
{
    extension(IExpressionContextSetup options)
    {
        public void UseKingbaseES(string masterConnectString, params string[] slaveConnectStrings)
        => options.UseKingbaseES("MainDb", masterConnectString, slaveConnectStrings);
        public void UseKingbaseES(string? key, string masterConnectString, params string[] slaveConnectStrings)
        {
            UseKingbaseES(options, set =>
            {
                set.DbKey = key;
                set.MasterConnectionString = masterConnectString;
                set.SalveConnectionStrings = slaveConnectStrings;
            });
        }
        public void UseKingbaseES(Action<IDbOption> setting)
        {
            var dbOption = new DataBaseOption<KingbaseESTableOptions>();
            setting.Invoke(dbOption);
            if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
            {
                throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
            }
            var provider = KingbaseESProvider.Create(dbOption);
            options.SetDatabase(dbOption.DbKey ?? "MainDb", KingbaseESProvider.KingbaseEs, provider);
        }

        /// <summary>
        /// 只登记"从配置创建 KingbaseES provider"的工厂，不注册任何实例。
        /// <para>配合容器里的 <c>IConfiguration</c>（配置节 <c>LightOrmDatabases</c>）使用：配置里 <c>Type = "KingbaseES"</c> 的条目才会被创建。</para>
        /// </summary>
        public void UseKingbaseES()
        {
            options.RegisterDatabase(KingbaseESProvider.KingbaseEs, database => KingbaseESProvider.Create(database));
        }
    }

    extension(IDbOption option)
    {
        public void ConfigureKbes(Action<KingbaseESTableOptions> config)
        {
            config.Invoke(option.GetOption<KingbaseESTableOptions>());
        }
    }
}
