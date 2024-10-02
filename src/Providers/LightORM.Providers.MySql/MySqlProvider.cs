using LightORM.Interfaces;
using System.Data.Common;

namespace LightORM.Providers.MySql;

public sealed class MySqlProvider : IDatabaseProvider
{
    public DbBaseType DbBaseType => DbBaseType.MySql;
    public string MasterConnectionString { get; }

    public ICustomDatabase CustomDatabase { get; } = CustomMySql.Instance;

    public Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; } = option => new MySqlTableHandler(option);

    public string[] SlaveConnectionStrings { get; }

    public DbProviderFactory DbProviderFactory { get; } = MySqlConnector.MySqlConnectorFactory.Instance;

    private MySqlProvider(string master, params string[] slaves)
    {
        MasterConnectionString = master;
        SlaveConnectionStrings = slaves;
    }

    public static MySqlProvider Create(string master, params string[] slaves) => new MySqlProvider(master, slaves);
}
