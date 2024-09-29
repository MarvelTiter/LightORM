using LightORM.Interfaces;
using System.Data.Common;

namespace LightORM.Providers.Oracle;

public sealed class OracleProvider : IDatabaseProvider
{
    public string MasterConnectionString { get; }

    public ICustomDatabase CustomDatabase { get; }

    public Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; }

    public string[] SlaveConnectionStrings { get; }

    public DbProviderFactory DbProviderFactory { get; }
    public OracleProvider(ICustomDatabase customDatabase
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
