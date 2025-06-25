using LightORM.Interfaces;
using LightORM.Models;
namespace LightORM.Providers.MySql.Extensions;

public static class IoCExtension
{
    public static void UseMySql(this IExpressionContextSetup options, string masterConnectString, params string[] slaveConnectStrings)
        => options.UseMySql("MainDb", masterConnectString, slaveConnectStrings);
    public static void UseMySql(this IExpressionContextSetup options, string? key, string masterConnectString, params string[] slaveConnectStrings)
    {
        var provider = MySqlProvider.Create(masterConnectString, slaveConnectStrings);
        options.SetDatabase(key, DbBaseType.MySql, provider);
    }
    public static void UseMySql(this IExpressionContextSetup options, Action<IDbOption> setting)
    {
        var dbOption = new DataBaseOption(CustomMySql.Instance.MethodResolver);
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        var provider = MySqlProvider.Create(dbOption.MasterConnectionString!, dbOption.SalveConnectionStrings ?? []);
        if (dbOption.NewFactory is not null)
        {
            provider.DbProviderFactory = dbOption.NewFactory;
        }
        options.SetDatabase(dbOption.DbKey ?? "MainDb", DbBaseType.MySql, provider);
    }
}
