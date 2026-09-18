using LightORM.Builder;
using LightORM.Extension;
using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Models;
using System.Text;

namespace LightORM.Providers.MySql;

internal sealed partial class CustomMySqlAdapter(ISqlMethodResolver methodResolver, MySqlCapabilities capabilities) : CustomDatabaseAdapter(methodResolver), IReturnIdentity
{
    internal static readonly CustomMySqlAdapter Instance = new(new MySqlMethodResolver(), MySqlCapabilities.Baseline);
    public MySqlCapabilities Capabilities { get; } = capabilities;
    public override string Prefix => "?";
    public override string Emphasis => "``";
    public override void Paging(SelectBuilder builder, StringBuilder sql)
    {
        sql.AppendLine($"LIMIT {builder.Skip}, {builder.Take}");
    }

    public override void HandleSelectGroupBySegment(SelectContext context)
    {
        if (Capabilities.Features.HasFlag(MySqlFeatures.GroupByRollup))
        {
            base.HandleSelectGroupBySegment(context);
            return;
        }

        // 8.0 以下：ROLLUP 只有旧写法 `GROUP BY (...) WITH ROLLUP`；
        // CUBE / GROUPING SETS 没有替代实现 → 不判版本，走 base 生成后交给数据库报错（与"只有一种实现的函数"同一原则）。
        var builder = context.Builder;
        if (builder.IsRollup)
        {
            var sql = context.Sql;
            sql.Append(context.Ident).Append("GROUP BY (").Append(builder.GroupBy).AppendLine(") WITH ROLLUP");
            return;
        }
        base.HandleSelectGroupBySegment(context);
    }
    public void ReturnIdentitySql(StringBuilder sql) => sql.Append("SELECT @@IDENTITY");

    public override void HandleDateValue(StringBuilder sql, DateTime dateTime)
    {
        //STR_TO_DATE('', '%Y-%m-%d %H:%i:%s')
        sql.Append("STR_TO_DATE('");
        sql.Append(dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        sql.Append("', '%Y-%m-%d %H:%i:%s')");
    }

    public override void HandleJsonColumn(JsonColumnContext context)
    {
        if (context.Options.SqlType == SqlPartial.Update)
        {
            context.Sql.AppendEmphasis(context.Column.ColumnName, this);
            context.Sql.Append(" = ");
            // 无索引、无路径成员(如 Set(j => j.Data, obj)) = 整列替换, 走列直接赋值。
            // 不能用 JSON_SET(col,'$',val) 代替: MySQL 的 JSON 函数在首参为 NULL 时整体返回 NULL,
            // 列原本为空时整列写回会被静默丢弃(实测确认)。参数已是 JSON 文本, 写入 JSON 列时 MySQL 会自动解析,
            // 与 Sqlite / SqlServer / Dameng / PG 的整列分支保持一致。
            if (!context.HasIndexInfo())
            {
                context.Sql.Append(Prefix);
                context.Sql.Append(context.Column.PropertyName);
                return;
            }
            context.Sql.Append("JSON_SET");
            context.Sql.Append('(');
            if (context.Options.RequiredTableAlias)
            {
                context.Sql.Append(context.Table.Alias);
                context.Sql.Append('.');
            }
            context.Sql.AppendEmphasis(context.Column.ColumnName, this);
            context.Sql.Append(",'$");
            BuildJsonPath();
            context.Sql.Append("',");
            // 更新还有第三个参数: 参数值是框架序列化后的 JSON 文本, 而 JSON_SET 会把字符串参数按
            // JSON 字符串字面量处理、不会解析为 JSON 值, 直接使用会双重编码(存成 "\"abc\"");
            // 故须 CAST(... AS JSON) 显式解析, 与 PG 的 @p::JSONB / Sqlite 的 JSON(@p) 语义对齐。
            context.Sql.Append("CAST(");
            context.Sql.Append(Prefix);
            context.Sql.Append(context.Column.PropertyName);
            context.Sql.Append(" AS JSON)");
            // 结束
            context.Sql.Append(')');
        }
        else
        {
            if (context.Options.RequiredTableAlias)
            {
                context.Sql.Append(context.Table.Alias);
                context.Sql.Append('.');
            }
            context.Sql.AppendEmphasis(context.Column.ColumnName, this);
            context.Sql.Append("->>");
            context.Sql.Append("'$");
            BuildJsonPath();
            context.Sql.Append('\'');
        }

        void BuildJsonPath()
        {
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
        }

    }
}
