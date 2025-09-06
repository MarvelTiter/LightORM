using LightORM.DbStruct;
using LightORM.Implements;
using System.Text;
using LightORM.Providers.Sqlite.TableStructure;

namespace LightORM.Providers.Sqlite;

public sealed class SqliteTableHandler 
    : BaseDatabaseHandler<SqliteTableWriter>
{
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