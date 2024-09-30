using LightORM.Interfaces;
using Oracle.ManagedDataAccess.Client;
using System.Data.Common;

namespace LightORM.Providers.Oracle;

public sealed class OracleProvider : IDatabaseProvider
{
    public string MasterConnectionString { get; }

    public ICustomDatabase CustomDatabase { get; } = CustomOracle.Instance;

    public Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; } = option => new OracleTableHandler(option);

    public string[] SlaveConnectionStrings { get; }

    public DbProviderFactory DbProviderFactory { get; } = OracleClientFactory.Instance;

    public OracleProvider(string master, params string[] slaves)
    {
        MasterConnectionString = master;
        SlaveConnectionStrings = slaves;
    }

    public static OracleProvider Create(string master, params string[] slaves) => new OracleProvider(master, slaves);
}
