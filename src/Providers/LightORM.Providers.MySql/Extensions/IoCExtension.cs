
namespace LightORM.Providers.MySql.Extensions;

public static class IoCExtension
{
    public static void UseMySql(this ExpressionSqlOptions options, string masterConnectString, params string[] slaveConnectStrings)
        => options.UseMySql("MainDb", masterConnectString, slaveConnectStrings);
    public static void UseMySql(this ExpressionSqlOptions options, string? key, string masterConnectString, params string[] slaveConnectStrings)
    {
        var provider = MySqlProvider.Create(masterConnectString, slaveConnectStrings);
        options.SetDatabase(key, DbBaseType.MySql, provider);
    }
}
