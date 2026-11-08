using LightORM.Interfaces;
using LightORM.Models;

namespace LightORM.Providers.SqlServer.Extensions;

public static class IoCExtension
{
    extension(IExpressionContextSetup options)
    {
        // 需要指定 DbKey / 显式版本 / 关闭版本探测时，用 Action 重载：
        //   options.UseSqlServer(set => { set.DbKey = "Log"; set.MasterConnectionString = cs;
        //       set.ConfiguraSqlServer(o => o.DetectVersion = false); });
        public void UseSqlServer(string masterConnectString, params string[] slaveConnectStrings)
        => options.UseSqlServer(set =>
        {
            set.DbKey = "MainDb";
            set.MasterConnectionString = masterConnectString;
            set.SalveConnectionStrings = slaveConnectStrings;
        });

        public void UseSqlServer(Action<IDbOption> setting)
        {
            var dbOption = new DataBaseOption<SqlServerTableOptions>();
            setting.Invoke(dbOption);
            if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
            {
                throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
            }
            var provider = SqlServerProvider.Create(dbOption);
            options.SetDatabase(dbOption.DbKey ?? "MainDb", DbBaseType.SqlServer, provider);
        }

        /// <summary>
        /// 只登记"从配置创建 SqlServer provider"的工厂，不注册任何实例。
        /// <para>配合 <c>AddLightOrm(..., IConfiguration)</c> 使用：配置里 <c>Type = "SqlServer"</c> 的条目才会被创建。</para>
        /// </summary>
        public void UseSqlServer()
        {
            options.RegisterDatabase(DbBaseType.SqlServer, database => SqlServerProvider.Create(database));
        }
    }

    extension(IDbOption option)
    {
        public void ConfiguraSqlServer(Action<SqlServerTableOptions> config)
        {
            config.Invoke(option.GetOption<SqlServerTableOptions>());
        }
    }
}
