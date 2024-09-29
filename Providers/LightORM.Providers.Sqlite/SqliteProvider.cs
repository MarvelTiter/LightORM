using LightORM.Interfaces;
using System.Data.Common;

namespace LightORM.Providers.Sqlite;

public sealed class SqliteProvider : IDatabaseProvider
{
    public DbProviderFactory DbProviderFactory { get; }

    public string MasterConnectionString { get; }

    public string[] SlaveConnectionStrings { get; }

    public ICustomDatabase CustomDatabase { get; }

    public Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; }

    public SqliteProvider(ICustomDatabase customDatabase
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
