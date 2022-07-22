using System.Text;

namespace MDbContext.ExpressionSql.DbHandle;

internal interface IDbHelper
{
    string GetPrefix();
    string GetColumnEmphasis(bool isLeft);
    /// <summary>
    /// 防止与关键字冲突
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    string ColumnEmphasis(string columnName);
    string DbStringConvert(string content);
    void DbPaging(SqlContext context, SqlFragment select, StringBuilder sql, int index, int size);
}
