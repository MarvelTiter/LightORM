using LightORM;
using LightORM.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace LightORM.Providers.SqlServer;

public enum SqlServerVersion
{
    V1,
    Over2012,
    Over2017,
}

public sealed class SqlServerProvider : IDatabaseProvider
{
    public string MasterConnectionString { get; }

    public ICustomDatabase CustomDatabase { get; }

    public Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; } = option => new SqlServerTableHandler(option);

    public string[] SlaveConnectionStrings { get; }

    public DbProviderFactory DbProviderFactory { get; } = SqlClientFactory.Instance;
    public SqlServerProvider(SqlServerVersion version
        , string master
        , params string[] slaves)
    {
        CustomDatabase = new CustomSqlServer(version);
        MasterConnectionString = master;
        SlaveConnectionStrings = slaves;
    }

    public static SqlServerProvider Create(SqlServerVersion version, string master, params string[] slaves)
        => new SqlServerProvider(version, master, slaves);
    
}
