using System.Text;

namespace MDbContext.ExpressionSql.DbHandle;
internal class OracleDb : IDbHelper
{
    // oracle用""，字段名需要大写
    public string DbEmphasis(string columnName) => $"\"{columnName.ToUpper()}\"";

    public void DbPaging(SqlContext context, SqlFragment select, StringBuilder sql, int index, int size)
    {
        sql.Insert(0, " SELECT ROWNUM as ROWNO, SubMax.* FROM (\n ");
        sql.Append(" \n) SubMax WHERE ROWNUM <= ");
        sql.Append(index * size);
        sql.Insert(0, " SELECT * FROM (\n ");
        sql.Append(" \n) SubMin WHERE SubMin.ROWNO > ");
        sql.Append((index - 1) * size);
    }

    public string DbStringConvert(string content) => $"TO_CHAR({content})";

    public string GetColumnEmphasis(bool isLeft) => "\"";

    public string GetPrefix() => ":";
}
