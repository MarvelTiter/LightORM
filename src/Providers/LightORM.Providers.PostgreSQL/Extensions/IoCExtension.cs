using LightORM.Interfaces;
using LightORM.Models;

namespace LightORM.Providers.PostgreSQL.Extensions;

public static class IoCExtension
{
    public static void UsePostgreSQL(this IExpressionContextSetup options, string masterConnectString, params string[] slaveConnectStrings)
        => options.UsePostgreSQL("MainDb", masterConnectString, slaveConnectStrings);
    public static void UsePostgreSQL(this IExpressionContextSetup options, string? key, string masterConnectString, params string[] slaveConnectStrings)
    {
        var provider = PostgreSQLProvider.Create(masterConnectString, slaveConnectStrings);
        options.SetDatabase(key, DbBaseType.PostgreSQL, provider);
    }
    public static void UsePostgreSQL(this IExpressionContextSetup options, Action<IDbOption> setting)
    {
        var dbOption = new DataBaseOption(CustomPostgreSQL.Instance.MethodResolver);
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        var provider = PostgreSQLProvider.Create(dbOption.MasterConnectionString!, dbOption.SalveConnectionStrings ?? []);
        if (dbOption.NewFactory is not null)
        {
            provider.DbProviderFactory = dbOption.NewFactory;
        }
        options.SetDatabase(dbOption.DbKey ?? "MainDb", DbBaseType.PostgreSQL, provider);
    }
}
