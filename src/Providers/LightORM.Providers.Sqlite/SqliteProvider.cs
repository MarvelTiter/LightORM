using LightORM.Interfaces;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using LightORM.Implements;

namespace LightORM.Providers.Sqlite;

public sealed class SqliteProvider : BaseDatabaseProvider
{
    public static SqliteProvider Create(string master, params string[] slaves) => new SqliteProvider(master, slaves);
    private SqliteProvider(string master, params string[] slaves):base(master,slaves)
    {
    }
    public override DbBaseType DbBaseType => DbBaseType.Sqlite;
    public override DbProviderFactory DbProviderFactory { get; set; } = SQLiteFactory.Instance;

    public override ICustomDatabase CustomDatabase { get; } = CustomSqlite.Instance;

    public override Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; } = option => throw new NotSupportedException();
    public override IDatabaseTableHandler DbHandler { get; } = new SqliteTableHandler();

    public override int BulkCopy(DataTable dataTable)
    {
        throw new NotSupportedException();
    }


}
