﻿using LightORM.Implements;
using LightORM.Interfaces;
using System.Text;

namespace LightORM.Providers.SqlServer;

public sealed class CustomSqlServer(SqlServerVersion version) : CustomDatabase(new SqlServerMethodResolver(version))
{
    public SqlServerVersion Version { get; } = version;
    public override string Prefix => "@";
    public override string Emphasis => "[]";

    public override void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        if (Version == SqlServerVersion.Over2012)
        {
            sql.AppendLine($"OFFSET {(builder.PageIndex - 1) * builder.PageSize} ROWS");
            sql.AppendLine($"FETCH NEXT {builder.PageSize} ROWS ONLY");
        }
        else
        {
            var orderByString = "";
            var orderByType = "";
            if (builder.OrderBy.Count == 0)
            {
                var col = builder.MainTable.TableEntityInfo.Columns.First(c => c.IsPrimaryKey);
                orderByString = $"Sub.{col.ColumnName}";
                orderByType = " ASC";
            }
            else
            {
                orderByString = string.Join(",", builder.OrderBy.Select(s => s.Split('.')[1]));
                orderByType = (builder.AdditionalValue == null ? "" : $" {builder.AdditionalValue}");
            }
            sql.Insert(6, " TOP (100) PERCENT");
            sql.Insert(0, $" SELECT ROW_NUMBER() OVER(ORDER BY {orderByString}{orderByType}) ROWNO, Sub.* FROM ({Environment.NewLine} ");
            sql.AppendLine("  ) Sub");
            // 子查询筛选 ROWNO
            sql.Insert(0, $" SELECT * FROM ({Environment.NewLine} ");
            sql.AppendLine("  ) Paging");
            sql.AppendLine($" WHERE Paging.ROWNO > {(builder.PageIndex - 1) * builder.PageSize}");
            sql.Append($" AND Paging.ROWNO <= {builder.PageIndex * builder.PageSize}");
        }
    }

    public override string ReturnIdentitySql() => "SELECT SCOPE_IDENTITY()";
}
