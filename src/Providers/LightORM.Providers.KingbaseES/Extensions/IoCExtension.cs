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
            options.SetDatabase(dbOption.DbKey ?? "MainDb", DbBaseType.Oracle, provider);
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
