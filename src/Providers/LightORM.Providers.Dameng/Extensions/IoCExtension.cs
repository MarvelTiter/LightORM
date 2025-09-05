using LightORM.Interfaces;
using LightORM.Models;
namespace LightORM.Providers.Dameng.Extensions;

public static class IoCExtension
{
    // TODO 使用 DbBaseType.Dameng
    private static readonly DbBaseType Dameng = new("Dameng");
    public static void UseDameng(this IExpressionContextSetup options, string masterConnectString, params string[] slaveConnectStrings)
        => options.UseDameng("MainDb", masterConnectString, slaveConnectStrings);
    public static void UseDameng(this IExpressionContextSetup options, string? key, string masterConnectString, params string[] slaveConnectStrings)
    {
        var provider = DamengProvider.Create(masterConnectString, slaveConnectStrings);
        options.SetDatabase(key, Dameng, provider);
    }
    public static void UseDameng(this IExpressionContextSetup options, Action<IDbOption> setting)
    {
        var dbOption = new DataBaseOption(CustomDameng.Instance.MethodResolver);
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        var provider = DamengProvider.Create(dbOption.MasterConnectionString!, dbOption.SalveConnectionStrings ?? []);
        if (dbOption.NewFactory is not null)
        {
            provider.DbProviderFactory = dbOption.NewFactory;
        }
        options.SetDatabase(dbOption.DbKey ?? "MainDb", Dameng, provider);
    }
}
