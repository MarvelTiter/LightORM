using LightORM.Builder;
using LightORM.Extension;
using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Models;
using System.Text;

namespace LightORM.Providers.MySql;

#pragma warning disable CS9113 // 参数未读。
internal sealed partial class CustomMySqlAdapter(ISqlMethodResolver methodResolver, MySqlTableOptions tableOptions) : CustomDatabaseAdapter(methodResolver)
{
    internal static readonly CustomMySqlAdapter Instance = new(new MySqlMethodResolver(), new());
    public override string Prefix => "?";
    public override string Emphasis => "``";
    public override void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        sql.AppendLine($"LIMIT {builder.Skip}, {builder.Take}");
    }

    public override void HandleSelectGroupBySegment(SelectContext context)
    {
        var over8 = tableOptions.Version > new Version(8, 0);
        if (over8)
        {
            base.HandleSelectGroupBySegment(context);
        }
        else
        {
            var sql = context.Sql;
            var builder = context.Builder;
            var ident = context.Ident;
            if (builder.IsRollup)
            {
                // $"{ident}GROUP BY ROLLUP ({string.Join(", ", GroupBy)})"
                sql.Append(ident).Append("GROUP BY (").Append(builder.GroupBy).AppendLine(") WITH ROLLUP");
            }
            else if (builder.IsCube)
            {
                throw new NotSupportedException("MySQL 8.0 以下版本不支持 CUBE 分组");
            }
            else if (builder.GroupingSets.Count > 0)
            {
                throw new NotSupportedException("MySQL 8.0 以下版本不支持 GROUPING SETS 分组");
            }
            else
            {
                // $"{ident}GROUP BY {string.Join(", ", GroupBy)}"
                sql.Append(ident).Append("GROUP BY ").Append(builder.GroupBy).AppendLine();
            }
        }
    }
    public override void ReturnIdentitySql(StringBuilder sql) => sql.Append("SELECT @@IDENTITY");

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
            // 更新还有第三个参数
            context.Sql.Append(Prefix);
            context.Sql.Append(context.Column.PropertyName);
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
