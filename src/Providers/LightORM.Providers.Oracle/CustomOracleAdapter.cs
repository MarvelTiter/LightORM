using LightORM.Builder;
using LightORM.Extension;
using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Models;
using LightORM.Utils;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.Common;
using System.Text;

namespace LightORM.Providers.Oracle;

#pragma warning disable CS9113 // 参数未读。
internal sealed partial class CustomOracleAdapter(ISqlMethodResolver methodResolver, OracleTableOptions tableOptions) : CustomDatabaseAdapter(methodResolver), IDbCommandInitializer
{
    internal readonly static CustomOracleAdapter Instance = new(new OracleMethodResolver(), new());
    public override string Prefix => ":";
    public override string Emphasis => "\"\"";

    public void DbCommandInit(DbCommand dbCommand)
    {
        if (dbCommand is OracleCommand oracleCommand)
        {
            oracleCommand.BindByName = true;
            if (tableOptions.InitialLONGFetchSize.HasValue)
                oracleCommand.InitialLONGFetchSize = tableOptions.InitialLONGFetchSize.Value;
            //oracleCommand.InitialLOBFetchSize = -1;
        }
    }

    public override void Paging(SelectBuilder builder, StringBuilder sql)
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
        using var _ = StringBuilderPool.Get(out var sb);
        sb.AppendLine("BEGIN");

        for (int i = 0; i < sqls.Length; i++)
        {
            var sql = sqls[i];
            if (string.IsNullOrWhiteSpace(sql))
                continue;
            string cursorName = $"cur{i}";
            // 追加游标打开语句，注意 SQL 中的参数已经重写过，直接嵌入即可
            sb.AppendLine($"    OPEN :{cursorName} FOR {sql};");
            // 创建输出游标参数
            var cursorParam = new OracleParameter(cursorName, OracleDbType.RefCursor, ParameterDirection.Output);
            // 添加到 parameters 字典
            parameters[cursorName] = cursorParam;
        }

        sb.AppendLine("END;");
        return sb.ToString();
    }

    public override bool BindParameter(IDataParameter parameter, ITableColumnInfo? column, object? value)
    {
        if (column?.IsJsonColumn == true && parameter is OracleParameter oracleParameter)
        {
            // json 列参数统一序列化为 JSON 文本后按 CLOB 绑定:
            //   · Oracle 的 VARCHAR2 绑定有 4000 字节上限, 超长 JSON 文档必须走 CLOB(否则 ORA-01461);
            //   · 文本参数在 JSON_TRANSFORM 的 SET 中按 JSON 字符串处理, 由 SQL 侧的 JSON(:p) 负责解析,
            //     故此处只需要把它序列化成合法 JSON 文本(与 SQLite 的 JSON(@p) 同一约定)。
            // CLOB 在隐式转换到指定 JSON 列类型时同样有效(JSON() 接受 VARCHAR2/CLOB/BLOB)。
            oracleParameter.OracleDbType = OracleDbType.Clob;
            oracleParameter.Value = value is null ? DBNull.Value : JsonParameterHelper.Serialize(value);
            return true;
        }
        return false;
    }

    public override void HandleJsonColumn(JsonColumnContext context)
    {
        if (context.Options.SqlType == SqlPartial.Update)
        {
            // 无索引、无路径成员的 json 列引用 = 整列替换(如 Set(j => j.Data, obj))。
            // 不能走 JSON_TRANSFORM(col, SET '$' = :p):
            //   1) JSON_TRANSFORM 对 NULL 输入返回 NULL, 列原值为空时值会静默丢失(与已修的 MySQL JSON_SET 同类);
            //   2) SET '$' 的文本参数被当作 JSON 字符串写入, 会双重编码。
            // 与 PG / Sqlite / SqlServer / MySQL / Dameng 的整列分支保持一致, 直接列赋值。
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
            // JSON_TRANSFORM 是 Oracle 21c 引入的路径级更新函数(19c 及以前只有整文档合并的 JSON_MERGEPATCH)。
            context.Sql.Append("JSON_TRANSFORM");
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
            // 值参数须经 JSON() 构造器按 JSON 解析: JSON_TRANSFORM 的 SET 右值若是文本, Oracle 会按
            // "JSON 字符串"写入(而非解析), 而框架下发的参数是序列化后的 JSON 文本(如 "abc" / {"a":1}),
            // 直接赋值会双重编码(存成 "\"abc\"")、读回带多余引号。
            // 与 SQLite 的 JSON(@p)、PG 的 @p::JSONB 语义对齐; 由 OracleMethodResolver 处理。
            context.Sql.Append("JSON(");
            context.Sql.Append(Prefix);
            context.Sql.Append(context.Column.PropertyName);
            context.Sql.Append(')');
            context.Sql.Append(')');
        }
        else
        {
            // 读取路径: 末级为复合文档(对象/数组)时必须用 JSON_QUERY —— JSON_VALUE 只返回标量,
            // 遇对象/数组返回 NULL(投影一个嵌套对象会读回空对象); JSON_QUERY 返回 JSON 文档文本,
            // 由读取侧反序列化。末级为标量仍用 JSON_VALUE(返回去引号后的标量文本)。
            // 判定只用生成期信息(表达式结构/列类型), 与表达式缓存兼容。
            context.Sql.Append(JsonParameterHelper.IsCompositeJsonLeaf(context) ? "JSON_QUERY" : "JSON_VALUE");
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
