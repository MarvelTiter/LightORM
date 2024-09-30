using Microsoft.Data.SqlClient;

namespace LightORM.Providers.SqlServer.Extensions;

public static class IoCExtension
{
    public static void UseSqlServer(this ExpressionSqlOptions options, SqlServerVersion version, string masterConnectString, params string[] slaveConnectStrings)
        => options.UseSqlServer("MainDb", version, masterConnectString, slaveConnectStrings);
    public static void UseSqlServer(this ExpressionSqlOptions options, string? key, SqlServerVersion version, string masterConnectString, params string[] slaveConnectStrings)
    {
        var provider = SqlServerProvider.Create((version), masterConnectString, slaveConnectStrings);
        options.SetDatabase(key, DbBaseType.SqlServer, provider);
    }
}
