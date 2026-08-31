using LightORM.DbStruct;
using LightORM.Implements;

namespace LightORM.Providers.Sqlite;

public sealed partial class SqliteTableHandler(SqliteTableOptions generateOption)
    : BaseDatabaseHandler<SqliteTableOptions>
{
    public override SqliteTableOptions Options => generateOption;

    public override string GetTablesSql()
    {
        throw new NotImplementedException();
    }

    public override string GetTableStructSql(string table)
    {
        throw new NotImplementedException();
    }

    public override bool ParseDataType(ReadedTableColumn column, out string type)
    {
        throw new NotImplementedException();
    }
}