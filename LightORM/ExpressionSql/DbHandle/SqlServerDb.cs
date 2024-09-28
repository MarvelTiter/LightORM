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
        sql.AppendLine($"OFFSET {(builder.PageIndex - 1) * builder.PageSize} ROWS");
        sql.AppendLine($"FETCH NEXT {builder.PageSize} ROWS ONLY");
    }

    public string ReturnIdentitySql() => "SELECT SCOPE_IDENTITY()";
}
internal class SqlServerDb : IDbHelper
{
    public void Paging(SelectBuilder builder, StringBuilder sql)
    {
        var orderByString = "";
        var orderByType = "";
        if (builder.OrderBy.Count == 0)
        {
            var col = builder.MainTable.Columns.First(c => c.IsPrimaryKey);
            orderByString = $"Sub.{col.ColumnName}";
            orderByType = " ASC";
        }
        else
        {
            orderByString = string.Join(",", builder.OrderBy.Select(s => s.Split('.')[1]));
            orderByType = (builder.AdditionalValue == null ? "" : $" {builder.AdditionalValue}");
        }
        sql.Insert(6, " TOP (100) PERCENT");
        sql.Insert(0, $" SELECT ROW_NUMBER() OVER(ORDER BY {orderByString}{orderByType}) ROWNO, Sub.* FROM ({SqlBuilder.N} ");
        sql.AppendLine("  ) Sub");
        // 子查询筛选 ROWNO
        sql.Insert(0, $" SELECT * FROM ({SqlBuilder.N} ");
        sql.AppendLine("  ) Paging");
        sql.AppendLine($" WHERE Paging.ROWNO > {(builder.PageIndex - 1) * builder.PageSize}");
        sql.Append($" AND Paging.ROWNO <= {builder.PageIndex * builder.PageSize}");
    }

    public string ReturnIdentitySql() => "SELECT SCOPE_IDENTITY()";
}
