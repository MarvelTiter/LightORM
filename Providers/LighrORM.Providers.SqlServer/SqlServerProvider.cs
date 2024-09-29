using LightORM;
using LightORM.Interfaces;
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

    public Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; }

    public string[] SlaveConnectionStrings { get; }

    public DbProviderFactory DbProviderFactory { get; }
    public SqlServerProvider(ICustomDatabase customDatabase
        , Func<TableGenerateOption, IDatabaseTableHandler>? tableHandler
        , DbProviderFactory factory
        , string master
        , params string[] slaves)
    {
        CustomDatabase = customDatabase;
        TableHandler = tableHandler;
        DbProviderFactory = factory;
        MasterConnectionString = master;
        SlaveConnectionStrings = slaves;
    }
    
}
