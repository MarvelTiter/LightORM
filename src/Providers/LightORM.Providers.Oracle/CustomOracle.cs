using LightORM.Implements;
using LightORM.Interfaces;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Text;

namespace LightORM.Providers.Oracle;

public sealed class CustomOracle(): CustomDatabase(new OracleMethodResolver())
{
    internal readonly static CustomOracle Instance = new CustomOracle();
    public override string Prefix => ":";
    public override string Emphasis => "\"\"";
    public override void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        sql.Insert(0, $"SELECT ROWNUM as ROWNO, SubMax.* FROM (\n");
        sql.AppendLine($") SubMax WHERE ROWNUM <= {builder.Skip + builder.Take}");
        sql.Insert(0, "SELECT * FROM (\n");
        sql.AppendLine($") SubMin WHERE SubMin.ROWNO > {builder.Skip}");
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
}
