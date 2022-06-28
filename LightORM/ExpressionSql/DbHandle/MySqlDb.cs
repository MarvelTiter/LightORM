using System.Text;

namespace MDbContext.ExpressionSql.DbHandle;

internal class MySqlDb : IDbHelper
{
    public void DbPaging(SqlContext context, SqlFragment select, StringBuilder sql, int index, int size)
    {
        sql.Append($"\nLIMIT {(index - 1) * size}, {size}");
    }

    public string DbStringConvert(string content) => $"CONVERT(VARCHAR, {content})";

    public string GetPrefix() => "?";
}
