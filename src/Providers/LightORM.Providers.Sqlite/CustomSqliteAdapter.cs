using LightORM.Builder;
using LightORM.Extension;
using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Models;
using System.Text;

namespace LightORM.Providers.Sqlite;

internal sealed partial class CustomSqliteAdapter(ISqlMethodResolver methodResolver, SqliteTableOptions options) : CustomDatabaseAdapter(methodResolver), IReturnIdentity
{
    /// <summary>
    /// 测试用
    /// </summary>
    internal readonly static CustomSqliteAdapter TestInstance = new(new SqliteMethodResolver(new()), new());
    public override string Prefix => "@";
    public override string Emphasis => "``";
    public override void Paging(SelectBuilder builder, StringBuilder sql)
    {
        sql.AppendLine($"LIMIT {builder.Skip}, {builder.Take}");
    }
    public void ReturnIdentitySql(StringBuilder sql) => sql.Append("SELECT LAST_INSERT_ROWID()");

    public override void HandleDateValue(StringBuilder sql, DateTime dateTime)
    {
        sql.Append(dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    string Extract => options.JSONBackend == JSONBackend.Binary ? "JSONB_EXTRACT" : "JSON_EXTRACT";
    string Set => options.JSONBackend == JSONBackend.Binary ? "JSONB_SET" : "JSON_SET";
    public override void HandleJsonColumn(JsonColumnContext context)
    {
        if (context.Options.SqlType == SqlPartial.Update)
        {
            if (!context.HasIndexInfo())
            {
                context.Sql.AppendEmphasis(context.Column.ColumnName, this);
                context.Sql.Append(" = ");
                context.Sql.Append(Prefix);
                context.Sql.Append(context.Column.PropertyName);
                return;
            }
            context.Sql.AppendEmphasis(context.Column.ColumnName, this);
            context.Sql.Append(" = ");
            context.Sql.Append(Set);
        }
        else
        {
            context.Sql.Append(Extract);
        }
        // 字段名称，属性路径都是一样的
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
        }
        context.Sql.Append('\'');
        if (context.Options.SqlType == SqlPartial.Update)
        {
            // 更新还有第三个参数。
            // 参数值是框架序列化后的 JSON 文本(如 "abc" / {"a":1})，
            // 而 SQLite 的 json_set/jsonb_set 对 TEXT 参数按"字符串字面量"处理、不会解析 JSON，
            // 直接使用会导致双重编码(存成 "\"abc\"")、读取带多余引号；
            // 故须经 JSON() 显式解析为 JSON 值，与 PG 的 @p::JSONB 语义对齐。
            context.Sql.Append(",JSON(");
            context.Sql.Append(Prefix);
            context.Sql.Append(context.Column.PropertyName);
            context.Sql.Append(')');
        }
        // 结束
        context.Sql.Append(')');
        
    }
}
