using System.Collections.Generic;
using System.Text;

namespace LightORM.ExpressionSql.DbHandle;
internal class OracleDb : IDbHelper
{
    public string BuildSelectSql(SqlContext context, SqlFragment? select, bool distanct, SqlFragment? where, SqlFragment? groupBy, SqlFragment? orderBy, bool isAsc, int index, int size)
    {
        var tables = context.Tables;
        var main = tables[0];
        StringBuilder sql = new StringBuilder();
        if (select == null)
        {
            sql.Append($"SELECT {(distanct ? "DISTINCT " : "")}* FROM {DbEmphasis(main.TableName!)} {main.Alias}");
        }
        else
        {
            select!.RemoveLastComma();
            sql.Append($"SELECT {(distanct ? "DISTINCT " : "")}{select} FROM {DbEmphasis(main.TableName!)} {main.Alias}");
        }
        for (int i = 1; i < context.Tables.Count; i++)
        {
            var temp = tables[i];
            if (temp.TableType == TableLinkType.None) continue;
            sql.Append($"\n{temp.TableType.ToLabel()} {DbEmphasis(temp.TableName!)} {temp.Alias} ON {temp.Fragment}");
        }
        if (where != null)
            sql.Append($"\nWHERE {where}");

        if (groupBy != null)
        {
            groupBy.RemoveLastComma();
            sql.Append($"\nGROUP BY {groupBy}");
        }

        if (orderBy != null)
        {
            orderBy.RemoveLastComma();
            sql.Append($"\nORDER BY {orderBy} {(isAsc ? "ASC" : "DESC")}");
        }

        if (index * size > 0)
        {
            //分页处理
            DbPaging(sql, index, size);
        }

        return sql.ToString();
    }

    // oracle用""，字段名需要大写
    public string DbEmphasis(string columnName) => $"\"{columnName.ToUpper()}\"";

    public void DbPaging(StringBuilder sql, int index, int size)
    {
        sql.Insert(0, " SELECT ROWNUM as ROWNO, SubMax.* FROM (\n ");
        sql.Append(" \n) SubMax WHERE ROWNUM <= ");
        sql.Append(index * size);
        sql.Insert(0, " SELECT * FROM (\n ");
        sql.Append(" \n) SubMin WHERE SubMin.ROWNO > ");
        sql.Append((index - 1) * size);
    }

    public string DbStringConvert(string content) => $"TO_CHAR({content})";

    public string GetColumnEmphasis(bool isLeft) => "\"";

    public string GetPrefix() => ":";
}
