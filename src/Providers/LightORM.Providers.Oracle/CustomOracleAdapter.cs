using LightORM.Extension;
using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Models;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Text;

namespace LightORM.Providers.Oracle;

public sealed class CustomOracleAdapter(ISqlMethodResolver methodResolver, TableOptions tableOptions) : CustomDatabaseAdapter(methodResolver)
{
    internal readonly static CustomOracleAdapter Instance = new(new OracleMethodResolver(), new());
    public override string Prefix => ":";
    public override string Emphasis => "\"\"";
    public override void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        sql.Insert(0, $"SELECT ROWNUM as ROWNO, SubMax.* FROM (\n");
        sql.AppendLine($") SubMax WHERE ROWNUM <= {builder.Skip + builder.Take}");
        sql.Insert(0, "SELECT * FROM (\n");
        sql.AppendLine($") SubMin WHERE SubMin.ROWNO > {builder.Skip}");
    }

    public override void HandleDateValue(StringBuilder sql, DateTime dateTime)
    {
        sql.Append("TO_DATE('");
        sql.Append(dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        sql.Append("', 'YYYY-MM-DD HH24:MI:SS')");
    }

    public override string HandleMultipleQuerySql(string[] sqls, Dictionary<string, object> parameters)
    {
        var sb = new StringBuilder();
        sb.AppendLine("BEGIN");

        for (int i = 0; i < sqls.Length; i++)
        {
            string cursorName = $"cur{i}";
            // 追加游标打开语句，注意 SQL 中的参数已经重写过，直接嵌入即可
            sb.AppendLine($"    OPEN :{cursorName} FOR {sqls[i]};");
            // 创建输出游标参数
            var cursorParam = new OracleParameter(cursorName, OracleDbType.RefCursor, ParameterDirection.Output);
            // 添加到 parameters 字典
            parameters[cursorName] = cursorParam;
        }

        sb.AppendLine("END;");
        return sb.ToString();
    }

    public override void HandleJsonColumn(JsonColumnContext context)
    {
        if (context.Options.SqlType == SqlPartial.Update)
        {
            context.Sql.AppendEmphasis(context.Column.ColumnName, this);
            context.Sql.Append(" = ");
            context.Sql.Append("JSON_TRANSFORM");
            //context.Sql.Append("JSON_MERGEPATCH");
            context.Sql.Append('(');
            if (context.Options.RequiredTableAlias)
            {
                context.Sql.Append(context.Table.Alias);
                context.Sql.Append('.');
            }
            context.Sql.AppendEmphasis(context.Column.ColumnName, this);
            context.Sql.Append(",SET '$");
            BuildJsonPath();
            context.Sql.Append('\'');
            context.Sql.Append('=');
            context.Sql.Append(Prefix);
            context.Sql.Append(context.Column.PropertyName);
            context.Sql.Append(')');
        }
        else
        {
            context.Sql.Append("JSON_VALUE");
            context.Sql.Append('(');
            if (context.Options.RequiredTableAlias)
            {
                context.Sql.Append(context.Table.Alias);
                context.Sql.Append('.');
            }
            context.Sql.AppendEmphasis(context.Column.ColumnName, this);
            context.Sql.Append(",'$");
            BuildJsonPath();
            context.Sql.Append("')");
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
