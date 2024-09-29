using Oracle.ManagedDataAccess.Client;

namespace LightORM.Providers.Oracle.Extensions;

public static class IoCExtension
{
    private static CustomOracle custom = new CustomOracle();
    public static void UseOracle(this ExpressionSqlOptions options, string masterConnectString, params string[] slaveConnectStrings)
        => options.UseOracle("MainDb", masterConnectString, slaveConnectStrings);
    public static void UseOracle(this ExpressionSqlOptions options, string? key, string masterConnectString, params string[] slaveConnectStrings)
    {
        var provider = new OracleProvider(custom
            , o => new OracleTableHandler(o)
            , OracleClientFactory.Instance
            , masterConnectString
            , slaveConnectStrings);
        options.SetDatabase(key, DbBaseType.Oracle, provider);
    }
}
