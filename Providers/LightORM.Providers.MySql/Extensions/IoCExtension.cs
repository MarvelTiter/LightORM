
namespace LightORM.Providers.MySql.Extensions;

public static class IoCExtension
{
    private static CustomMySql custom = new CustomMySql();
    public static void UseMySql(this ExpressionSqlOptions options, string masterConnectString, params string[] slaveConnectStrings)
        => options.UseMySql("MainDb", masterConnectString, slaveConnectStrings);
    public static void UseMySql(this ExpressionSqlOptions options, string? key, string masterConnectString, params string[] slaveConnectStrings)
    {
        var provider = new MySqlProvider(custom
            , o => new MySqlTableHandler(o)
            , MySqlConnector.MySqlConnectorFactory.Instance
            , masterConnectString
            , slaveConnectStrings);
        options.SetDatabase(key, DbBaseType.MySql, provider);
    }
}
