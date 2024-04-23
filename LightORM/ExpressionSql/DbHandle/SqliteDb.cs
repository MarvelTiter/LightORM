using LightORM.Builder;
using System.Text;

namespace LightORM.ExpressionSql.DbHandle;
internal class SqliteDb : IDbHelper
{
    public void Paging(SelectBuilder builder, StringBuilder sql)
    {
        sql.Append($"\nLIMIT {(builder.PageIndex - 1) * builder.PageSize}, {builder.PageSize}");
    }

    public string ReturnIdentitySql() => "SELECT LAST_INSERT_ROWID()";
}
