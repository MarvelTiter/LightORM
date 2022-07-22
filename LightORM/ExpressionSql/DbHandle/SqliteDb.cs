using System.Text;

namespace MDbContext.ExpressionSql.DbHandle;

internal class SqliteDb : IDbHelper
{
    public string ColumnEmphasis(string columnName) => $"`{columnName}`";

    public void DbPaging(SqlContext context, SqlFragment select, StringBuilder sql, int index, int size)
    {
        sql.Append($"\nLIMIT {(index - 1) * size}, {size}");
    }

    public string DbStringConvert(string content) => content;

    public string GetColumnEmphasis(bool isLeft) => "`";

    public string GetPrefix() => "@";
}
