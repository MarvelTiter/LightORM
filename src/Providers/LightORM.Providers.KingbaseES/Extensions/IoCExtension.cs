using LightORM.Interfaces;
using LightORM.Models;

namespace LightORM.Providers.KingbaseES.Extensions;

public static class IoCExtension
{
    public static void UseKingbaseES(this IExpressionContextSetup options, string masterConnectString, params string[] slaveConnectStrings)
        => options.UseKingbaseES("MainDb", masterConnectString, slaveConnectStrings);
    public static void UseKingbaseES(this IExpressionContextSetup options, string? key, string masterConnectString, params string[] slaveConnectStrings)
    {
        UseKingbaseES(options, set =>
        {
            set.DbKey = key;
            set.MasterConnectionString = masterConnectString;
            set.SalveConnectionStrings = slaveConnectStrings;
        });
    }
    public static void UseKingbaseES(this IExpressionContextSetup options, Action<IDbOption> setting)
    {
        var dbOption = new DataBaseOption();
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        var provider = KingbaseESProvider.Create(dbOption);
        options.SetDatabase(dbOption.DbKey ?? "MainDb", DbBaseType.Oracle, provider);
    }
}
