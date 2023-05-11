using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDbContext.ExpressionSql.DbHandle;

internal class SqlServerDbOver2012 : SqlSerberDbBase
{
    public override void DbPaging(SqlContext context, SqlFragment select, StringBuilder sql, int index, int size)
    {
        sql.Append($"\nOFFSET {(index - 1) * size} ROWS");
        sql.Append($"\nFETCH NEXT {size} ROWS ONLY");
    }
}
internal class SqlServerDb : SqlSerberDbBase
{
    public override void DbPaging(SqlContext context, SqlFragment select, StringBuilder sql, int index, int size)
    {
        var orderByField = select.Names[0];
        var aliasIndex = orderByField.IndexOf('.');
        sql.Insert(0, $" SELECT ROW_NUMBER() OVER(ORDER BY Sub.{orderByField.Remove(0, aliasIndex + 1)}) ROWNO, Sub.* FROM (\n ");
        sql.Append(" \n ) Sub");
        // 子查询筛选 ROWNO
        sql.Insert(0, " SELECT * FROM (\n ");
        sql.Append(" \n ) Paging");
        sql.Append($"\n WHERE Paging.ROWNO > {(index - 1) * size}");
        sql.Append($" AND Paging.ROWNO <= {index * size}");
    }
}
internal abstract class SqlSerberDbBase : IDbHelper
{
    public string DbEmphasis(string columnName) => $"[{columnName}]";

    public virtual void DbPaging(SqlContext context, SqlFragment select, StringBuilder sql, int index, int size)
    {
        throw new NotImplementedException();
    }

    public string DbStringConvert(string content) => $"CASE({content} as VARCHAR)";

    public string GetColumnEmphasis(bool isLeft) => isLeft ? "[" : "]";

    public string GetPrefix() => "@";
}
