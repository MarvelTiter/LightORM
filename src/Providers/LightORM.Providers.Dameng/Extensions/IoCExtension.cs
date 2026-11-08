using Dm;
using LightORM.Interfaces;
using LightORM.Models;
using LightORM.Options;
namespace LightORM.Providers.Dameng.Extensions;

public static class IoCExtension
{
    extension(IExpressionContextSetup options)
    {
        public void UseDameng(string masterConnectString, params string[] slaveConnectStrings)
        => options.UseDameng("MainDb", masterConnectString, slaveConnectStrings);
        public void UseDameng(string? key, string masterConnectString, params string[] slaveConnectStrings)
        {
            //var provider = DamengProvider.Create(masterConnectString, slaveConnectStrings);
            //options.SetDatabase(key, DbBaseType.Dameng, provider);
            UseDameng(options, set =>
            {
                set.DbKey = key;
                set.MasterConnectionString = masterConnectString;
                set.SalveConnectionStrings = slaveConnectStrings;
            });
        }
        public void UseDameng(Action<IDbOption> setting)
        {
            var dbOption = new DataBaseOption<DamengTableOptions>();
            setting.Invoke(dbOption);
            if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
            {
                throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
            }
            var provider = DamengProvider.Create(dbOption);
            options.SetDatabase(dbOption.DbKey ?? "MainDb", DamengProvider.Dameng, provider);
        }

        /// <summary>
        /// 只登记"从配置创建 Dameng provider"的工厂，不注册任何实例。
        /// <para>配合 <c>AddLightOrm(..., IConfiguration)</c> 使用：配置里 <c>Type = "Dameng"</c> 的条目才会被创建。</para>
        /// </summary>
        public void UseDameng()
        {
            options.RegisterDatabase(DamengProvider.Dameng, DamengProvider.Create);
        }
    }

    extension(IDbOption option)
    {
        public void ConfiguraDameng(Action<DamengTableOptions> config)
        {
            config.Invoke(option.GetOption<DamengTableOptions>());
        }
    }
}
