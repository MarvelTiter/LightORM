using LightORM.Interfaces;
using LightORM.Models;
using Microsoft.Data.SqlClient;

namespace LightORM.Providers.SqlServer.Extensions;

public static class IoCExtension
{
    public static void UseSqlServer(this IExpressionContextSetup options, SqlServerVersion version, string masterConnectString, params string[] slaveConnectStrings)
        => options.UseSqlServer("MainDb", version, masterConnectString, slaveConnectStrings);
    public static void UseSqlServer(this IExpressionContextSetup options, string? key, SqlServerVersion version, string masterConnectString, params string[] slaveConnectStrings)
    {
        var provider = SqlServerProvider.Create((version), masterConnectString, slaveConnectStrings);
        options.SetDatabase(key, DbBaseType.SqlServer, provider);
    }
    public static void UseSqlServer(this IExpressionContextSetup options, SqlServerVersion version, Action<IDbOption> setting)
    {
        var custom = new CustomSqlServer(version);
        var dbOption = new DataBaseOption(custom);
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        var provider = SqlServerProvider.Create(custom,dbOption.MasterConnectionString!, dbOption.SalveConnectionStrings ?? []);
        if (dbOption.NewFactory is not null)
        {
            provider.DbProviderFactory = dbOption.NewFactory;
        }
        options.SetDatabase(dbOption.DbKey ?? "MainDb", DbBaseType.SqlServer, provider);
    }
}
