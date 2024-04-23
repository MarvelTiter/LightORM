//using LightORM.ExpressionSql;
//using System;
//using System.Collections.Generic;
//using System.Linq;
using System.Linq;
using System.Text;
using LightORM.Builder;

namespace LightORM.ExpressionSql.DbHandle;

internal class SqlServerDbOver2012 : IDbHelper
{
    public void Paging(SelectBuilder builder, StringBuilder sql)
    {
        sql.Append($"\nOFFSET {(builder.PageIndex - 1) * builder.PageSize} ROWS");
        sql.Append($"\nFETCH NEXT {builder.PageSize} ROWS ONLY");
    }

    public string ReturnIdentitySql() => "SELECT SCOPE_IDENTITY()";
}
internal class SqlServerDb : IDbHelper
{
    public void Paging(SelectBuilder builder, StringBuilder sql)
    {
        var orderByString = "";
        if (builder.OrderBy.Count == 0)
        {
            var col = builder.TableInfo.Columns.First(c =>c.IsPrimaryKey);
            orderByString = $"Sub.{col.ColumnName} ASC";
        }
        else
        {
            orderByString = string.Join(",", builder.OrderBy.Select(s => s.Split('.')[1]));
        }
        sql.Insert(0, $" SELECT ROW_NUMBER() OVER(ORDER BY {orderByString}) ROWNO, Sub.* FROM (\n ");
        sql.Append(" \n ) Sub");
        // 子查询筛选 ROWNO
        sql.Insert(0, " SELECT * FROM (\n ");
        sql.Append(" \n ) Paging");
        sql.Append($"\n WHERE Paging.ROWNO > {(builder.PageIndex - 1) * builder.PageSize}");
        sql.Append($" AND Paging.ROWNO <= {builder.PageIndex * builder.PageSize}");
    }

    public string ReturnIdentitySql() => "SELECT SCOPE_IDENTITY()";
}
