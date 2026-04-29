using LightORM.Extension;
using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Models;
using Microsoft.Extensions.Options;
using System.Text;

namespace LightORM.Providers.Dameng;

public sealed class CustomDamengAdapter(ISqlMethodResolver methodResolver, TableOptions tableOptions) : CustomDatabaseAdapter(methodResolver)
{
    internal static readonly CustomDamengAdapter TestInstance = new CustomDamengAdapter(new DamengMethodResolver(new()), new());
    public override string Prefix => ":";
    public override string Emphasis => "\"\"";
    public override void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        sql.Insert(0, $"SELECT ROWNUM as ROWNO, SubMax.* FROM (\n");
        sql.AppendLine($") SubMax WHERE ROWNUM <= {builder.Skip + builder.Take}");
        sql.Insert(0, "SELECT * FROM (\n");
        sql.AppendLine($") SubMin WHERE SubMin.ROWNO > {builder.Skip}");
    }
    public override void ReturnIdentitySql(StringBuilder sql) => sql.Append("SELECT @@IDENTITY");

    public override void HandleDateValue(StringBuilder sql, DateTime dateTime)
    {
        // Dameng 使用 TO_DATE 函数来处理日期值
        sql.Append("TO_DATE('");
        sql.Append(dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        sql.Append("', 'YYYY-MM-DD HH24:MI:SS')");
    }

    string Extract => tableOptions.JSONBackend == JSONBackend.Binary ? "JSONB_VALUE" : "JSON_VALUE";
    string Set => tableOptions.JSONBackend == JSONBackend.Binary ? "JSONB_SET" : "JSON_SET";
    public override void HandleJsonColumn(JsonColumnContext context)
    {
        if (context.Options.SqlType == SqlPartial.Update)
        {
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
            // 更新还有第三个参数
            context.Sql.Append(',');
            context.Sql.Append(Prefix);
            context.Sql.Append(context.Column.PropertyName);
        }
        // 结束
        context.Sql.Append(')');
    }
}
