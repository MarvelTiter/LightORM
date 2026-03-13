using LightORM.Extension;
using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Interfaces.ExpSql;
using LightORM.Models;
using Microsoft.Extensions.Options;
using System.Text;

namespace LightORM.Providers.Sqlite;

public sealed class CustomSqlite(ISqlMethodResolver methodResolver, TableOptions options) : CustomDatabase(methodResolver)
{
    /// <summary>
    /// 测试用
    /// </summary>
    internal readonly static CustomSqlite Instance = new(new SqliteMethodResolver(), new());
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
            // TODO 处理更新语句
            context.Sql.Append(Set);
        }
        else
        {
            context.Sql.Append(Extract);
            context.Sql.Append('(');
            if (context.Options.RequiredTableAlias)
            {
                context.Sql.Append(context.Table.Alias);
                context.Sql.Append('.');
            }
            context.Sql.AppendEmphasis(context.Column.ColumnName, this);
            context.Sql.Append(",'$");
            if (context.Column.JsonRootType == JsonRootType.Object)
            {
                context.Sql.Append('.');
            }
            while (context.Members.Count > 0)
            {
                var mi = context.Members.Pop();
                if (mi.Member is not null)
                    context.Sql.Append(mi.Member.Name);
                if (mi.IndexValue.HasValue)
                {
                    mi.IndexValue.Format(context.Sql);
                }
                if (context.Members.Count > 0)
                    context.Sql.Append('.');
            }
            context.Sql.Append("')");
        }
    }
}
