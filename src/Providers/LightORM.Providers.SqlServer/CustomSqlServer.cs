using LightORM.Extension;
using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Models;
using System.Text;

namespace LightORM.Providers.SqlServer;

public sealed class CustomSqlServer(SqlServerVersion version, ISqlMethodResolver methodResolver, TableOptions tableOptions) : CustomDatabase(methodResolver)
{
    public SqlServerVersion Version { get; } = version;
    public override string Prefix => "@";
    public override string Emphasis => "[]";

    public override void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        if (Version == SqlServerVersion.Over2012)
        {
            sql.AppendLine($"OFFSET {builder.Skip} ROWS");
            sql.AppendLine($"FETCH NEXT {builder.Take} ROWS ONLY");
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
            sql.Insert(0, $"SELECT ROW_NUMBER() OVER(ORDER BY {orderByString}{orderByType}) ROWNO, Sub.* FROM (\n");
            sql.AppendLine("  ) Sub");
            // 子查询筛选 ROWNO
            sql.Insert(0, "SELECT * FROM (\n");
            sql.AppendLine(") Paging");
            sql.AppendLine($"WHERE Paging.ROWNO > {builder.Skip}");
            sql.Append($"AND Paging.ROWNO <= {builder.Skip + builder.Take}");
        }
    }
    public override string HandleBooleanValueForBulkCopy(bool value)
    {
        return value ? "true" : "false";
    }
    public override string ReturnIdentitySql() => "SELECT SCOPE_IDENTITY()";

    public override void HandleJsonColumn(JsonColumnContext context)
    {
        if (context.Options.SqlType == SqlPartial.Update)
        {
            context.Sql.AppendEmphasis(context.Column.ColumnName, this);
            context.Sql.Append(" = ");
            context.Sql.Append("JSON_MODIFY");
        }
        else
        {
            context.Sql.Append("JSON_VALUE");
        }
        context.Sql.Append('(');
        if (context.Options.RequiredTableAlias)
        {
            context.Sql.Append(context.Table.Alias);
            context.Sql.Append('.');
        }
        context.Sql.AppendEmphasis(context.Column.ColumnName, this);
        context.Sql.Append(",'$");
        while (context.Members.Count > 0)
        {
            var mi = context.Members.Pop();
            if (mi.Member is not null)
            {
                context.Sql.Append('.');
                context.Sql.Append(mi.Member.Name);
            }
            if (mi.IndexValue.HasValue)
            {
                mi.IndexValue.Format(i =>
                {
                    if (i.IsIntValue)
                    {
                        context.Sql.Append('[');
                        context.Sql.Append(i.IntValue);
                        context.Sql.Append(']');
                    }
                    else if (i.IsStringValue)
                    {
                        context.Sql.Append('.');
                        context.Sql.Append(i.StringValue);
                    }
                });
            }
            //if (context.Members.Count > 0)
            //{
            //    context.Sql.Append('.');
            //}
        }
        context.Sql.Append('\'');
        if (context.Options.SqlType == SqlPartial.Update)
        {
            // 更新还有第三个参数
            context.Sql.Append(',');
            context.Sql.Append(Prefix);
            context.Sql.Append(context.Column.PropertyName);
        }
        // 结束
        context.Sql.Append(')');
    }
}
