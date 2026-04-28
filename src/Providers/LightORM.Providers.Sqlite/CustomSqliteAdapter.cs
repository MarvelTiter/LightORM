using LightORM.Extension;
using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Interfaces.ExpSql;
using LightORM.Models;
using Microsoft.Extensions.Options;
using System.Text;

namespace LightORM.Providers.Sqlite;

public sealed class CustomSqliteAdapter(ISqlMethodResolver methodResolver, TableOptions options) : CustomDatabaseAdapter(methodResolver)
{
    /// <summary>
    /// 测试用
    /// </summary>
    internal readonly static CustomSqliteAdapter TestInstance = new(new SqliteMethodResolver(new()), new());
    public override string Prefix => "@";
    public override string Emphasis => "``";
    public override void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        sql.AppendLine($"LIMIT {builder.Skip}, {builder.Take}");
    }
    public override string ReturnIdentitySql() => "SELECT LAST_INSERT_ROWID()";

    string Extract => options.JSONBackend == JSONBackend.Binary ? "JSONB_EXTRACT" : "JSON_EXTRACT";
    string Set => options.JSONBackend == JSONBackend.Binary ? "JSONB_SET" : "JSON_SET";
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
        // 字段名称，属性路径都是一样的
        context.Sql.Append('(');
        if (context.Options.RequiredTableAlias)
        {
            context.Sql.Append(context.Table.Alias);
            context.Sql.Append('.');
        }
        context.Sql.AppendEmphasis(context.Column.ColumnName, this);
        context.Sql.Append(",'$");
        //if (context.Column.JsonRootType == JsonRootType.Object)
        //{
        //    context.Sql.Append('.');
        //}
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
