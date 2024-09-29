using Microsoft.Data.SqlClient;

namespace LightORM.Providers.SqlServer.Extensions;

public static class IoCExtension
{
    public static void UseSqlServer(this ExpressionSqlOptions options, SqlServerVersion version, string masterConnectString, params string[] slaveConnectStrings)
        => options.UseSqlServer("MainDb", version, masterConnectString, slaveConnectStrings);
    public static void UseSqlServer(this ExpressionSqlOptions options, string? key, SqlServerVersion version, string masterConnectString, params string[] slaveConnectStrings)
    {
        var provider = new SqlServerProvider(new CustomSqlServer(version)
            , o => new SqlServerTableHandler(o)
            , SqlClientFactory.Instance
            , masterConnectString
            , slaveConnectStrings);
        options.SetDatabase(key, DbBaseType.MySql, provider);
    }
}
