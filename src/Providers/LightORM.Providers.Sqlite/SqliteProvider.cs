using LightORM.Interfaces;
using System.Data.Common;
using System.Data.SQLite;

namespace LightORM.Providers.Sqlite;

public sealed class SqliteProvider : IDatabaseProvider
{
    public DbProviderFactory DbProviderFactory { get; } = SQLiteFactory.Instance;

    public string MasterConnectionString { get; }

    public string[] SlaveConnectionStrings { get; }

    public ICustomDatabase CustomDatabase { get; } = CustomSqlite.Instance;

    public Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; } = option => new SqliteTableHandler(option);

    //public static 

    public SqliteProvider(string master, params string[] slaves)
    {
        MasterConnectionString = master;
        SlaveConnectionStrings = slaves;
    }

    public static SqliteProvider Create(string master, params string[] slaves) => new SqliteProvider(master, slaves);
}
