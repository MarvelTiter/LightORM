using LightORM.Models;
using Oracle.ManagedDataAccess.Client;

namespace LightORM.Providers.Oracle.Extensions;

public static class IoCExtension
{
    public static void UseOracle(this ExpressionSqlOptions options, string masterConnectString, params string[] slaveConnectStrings)
        => options.UseOracle("MainDb", masterConnectString, slaveConnectStrings);
    public static void UseOracle(this ExpressionSqlOptions options, string? key, string masterConnectString, params string[] slaveConnectStrings)
    {
        var provider = OracleProvider.Create(masterConnectString, slaveConnectStrings);
        options.SetDatabase(key, DbBaseType.Oracle, provider);
    }
    public static void UseOracle(this ExpressionSqlOptions options, Action<DataBaseOption> setting)
    {
        var dbOption = new DataBaseOption(CustomOracle.Instance.MethodResolver);
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException("连接字符串不能为空");
        }
        var provider = OracleProvider.Create(dbOption.MasterConnectionString!, dbOption.SalveConnectionStrings ?? []);
        options.SetDatabase(dbOption.DbKey ?? "MainDb", DbBaseType.Oracle, provider);
    }
}
