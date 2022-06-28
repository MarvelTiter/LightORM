using System.Text;

namespace MDbContext.ExpressionSql.DbHandle;

internal interface IDbHelper
{
    string GetPrefix();
    string DbStringConvert(string content);
    void DbPaging(SqlContext context, SqlFragment select, StringBuilder sql, int index, int size);
}
