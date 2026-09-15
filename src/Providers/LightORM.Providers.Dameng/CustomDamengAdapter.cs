using LightORM.Builder;
using LightORM.Extension;
using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Models;
using System.Text;

namespace LightORM.Providers.Dameng;

internal sealed partial class CustomDamengAdapter(ISqlMethodResolver methodResolver, DamengTableOptions tableOptions) : CustomDatabaseAdapter(methodResolver), IReturnIdentity
{
    internal static readonly CustomDamengAdapter TestInstance = new CustomDamengAdapter(new DamengMethodResolver(new()), new());
    public override string Prefix => ":";
    public override string Emphasis => "\"\"";
    public override void Paging(SelectBuilder builder, StringBuilder sql)
    {
        sql.Insert(0, $"SELECT ROWNUM as ROWNO, SubMax.* FROM (\n");
        sql.AppendLine($") SubMax WHERE ROWNUM <= {builder.Skip + builder.Take}");
        sql.Insert(0, "SELECT * FROM (\n");
        sql.AppendLine($") SubMin WHERE SubMin.ROWNO > {builder.Skip}");
    }
    public void ReturnIdentitySql(StringBuilder sql) => sql.Append("SELECT @@IDENTITY");

    string Extract => tableOptions.JSONBackend == JSONBackend.Binary ? "JSONB_VALUE" : "JSON_VALUE";
    string Set => tableOptions.JSONBackend == JSONBackend.Binary ? "JSONB_SET" : "JSON_SET";
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
            // 达梦按 SQL 参数类型的规则把 JSON_SET 的第三参数转成 JSON 值:
            //   varchar -> JSON 字符串(原样, 不解析), number -> JSON 数字, 故 varchar 会被当字面量
            // 框架下发的是序列化后的 JSON 文本(string -> "string"), 必须显式 CAST 成 JSON 才会按 JSON 值解析。
            // 实测 CAST(:p AS JSON) 对 字符串/对象/数组/数字/布尔/null 均正确, 无需按标量/复合分流。
            context.Sql.Append(",CAST(");
            context.Sql.Append(Prefix);
            context.Sql.Append(context.Column.PropertyName);
            context.Sql.Append(" AS JSON)");
        }
        // 结束
        context.Sql.Append(')');
    }
}
