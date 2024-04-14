//using LightORM.ExpressionSql;
//using System;
//using System.Collections.Generic;
//using System.Linq;
using System.Linq;
using System.Text;
using LightORM.Builder;

namespace LightORM.ExpressionSql.DbHandle;

internal class SqlServerDbOver2012 : IDbHelper
{
    public void Paging(SelectBuilder builder, StringBuilder sql)
    {
        sql.Append($"\nOFFSET {(builder.PageIndex - 1) * builder.PageSize} ROWS");
        sql.Append($"\nFETCH NEXT {builder.PageSize} ROWS ONLY");
    }
}
//{
//    public override void DbPaging(StringBuilder sql, SqlFragment? orderby, bool isAsc, UnitCell? defaultCell, int index, int size)
//    {
//        sql.Append($"\nOFFSET {(index - 1) * size} ROWS");
//        sql.Append($"\nFETCH NEXT {size} ROWS ONLY");
//    }
//}
internal class SqlServerDb : IDbHelper
{
    public void Paging(SelectBuilder builder, StringBuilder sql)
    {
        var orderByString = "";
        if (builder.OrderBy.Count == 0)
        {
            var col = builder.TableInfo.Columns.First(c =>c.IsPrimaryKey);
            orderByString = $"Sub.{col.ColumnName} ASC";
        }
        else
        {
            orderByString = string.Join(",", builder.OrderBy.Select(s => s.Split('.')[1]));
        }
        sql.Insert(0, $" SELECT ROW_NUMBER() OVER(ORDER BY {orderByString}) ROWNO, Sub.* FROM (\n ");
        sql.Append(" \n ) Sub");
        // 子查询筛选 ROWNO
        sql.Insert(0, " SELECT * FROM (\n ");
        sql.Append(" \n ) Paging");
        sql.Append($"\n WHERE Paging.ROWNO > {(builder.PageIndex - 1) * builder.PageSize}");
        sql.Append($" AND Paging.ROWNO <= {builder.PageIndex * builder.PageSize}");
    }
}
//{
//    public override string BuildSelectSql(SqlContext context, SqlFragment? select, bool distanct, SqlFragment? where, SqlFragment? groupBy, SqlFragment? orderBy, bool isAsc, int index, int size)
//    {
//        var tables = context.Tables;
//        var main = tables[0];
//        StringBuilder sql = new StringBuilder();
//        if (select == null)
//        {
//            sql.Append($"SELECT {(distanct ? "DISTINCT " : "")}* FROM {DbEmphasis(main.TableName!)} {main.Alias}");
//        }
//        else
//        {
//            select!.RemoveLastComma();
//            sql.Append($"SELECT {(distanct ? "DISTINCT " : "")}{select} FROM {DbEmphasis(main.TableName!)} {main.Alias}");
//        }
//        for (int i = 1; i < context.Tables.Count; i++)
//        {
//            var temp = tables[i];
//            if (temp.TableType == TableLinkType.None) continue;
//            sql.Append($"\n{temp.TableType.ToLabel()} {DbEmphasis(temp.TableName!)} {temp.Alias} ON {temp.Fragment}");
//        }
//        if (where != null)
//            sql.Append($"\nWHERE {where}");

//        if (groupBy != null)
//        {
//            groupBy.RemoveLastComma();
//            sql.Append($"\nGROUP BY {groupBy}");
//        }

//        if (orderBy != null && index * size == 0)
//        {
//            orderBy.RemoveLastComma();
//            sql.Append($"\nORDER BY {orderBy} {(isAsc ? "ASC" : "DESC")}");
//        }

//        if (index * size > 0)
//        {
//            //分页处理
//            DbPaging(sql, orderBy, isAsc, select!.Cells[0], index, size);
//        }

//        return sql.ToString();
//    }
//    public override void DbPaging(StringBuilder sql, SqlFragment? orderby, bool isAsc, UnitCell? defaultCell, int index, int size)
//    {
//        var orderByString = "";
//        if (orderby == null)
//        {
//            orderByString = $"Sub.{defaultCell!.ColumnAlias} {(isAsc ? "ASC" : "DESC")}";
//        }
//        else
//        {
//            orderByString = string.Join(",", orderby.Cells.Select(c => $"Sub.{c.ColumnAlias} {(isAsc ? "ASC" : "DESC")}"));
//        }
//        sql.Insert(0, $" SELECT ROW_NUMBER() OVER(ORDER BY {orderByString}) ROWNO, Sub.* FROM (\n ");
//        sql.Append(" \n ) Sub");
//        // 子查询筛选 ROWNO
//        sql.Insert(0, " SELECT * FROM (\n ");
//        sql.Append(" \n ) Paging");
//        sql.Append($"\n WHERE Paging.ROWNO > {(index - 1) * size}");
//        sql.Append($" AND Paging.ROWNO <= {index * size}");
//    }
//}
//internal abstract class SqlServerDbBase : IDbHelper
//{
//    public virtual string BuildSelectSql(SqlContext context, SqlFragment? select, bool distanct, SqlFragment? where, SqlFragment? groupBy, SqlFragment? orderBy, bool isAsc, int index, int size)
//    {
//        var tables = context.Tables;
//        var main = tables[0];
//        StringBuilder sql = new StringBuilder();
//        if (select == null)
//        {
//            sql.Append($"SELECT {(distanct ? "DISTINCT " : "")}* FROM {DbEmphasis(main.TableName!)} {main.Alias}");
//        }
//        else
//        {
//            select!.RemoveLastComma();
//            sql.Append($"SELECT {(distanct ? "DISTINCT " : "")}{select} FROM {DbEmphasis(main.TableName!)} {main.Alias}");
//        }
//        for (int i = 1; i < context.Tables.Count; i++)
//        {
//            var temp = tables[i];
//            if (temp.TableType == TableLinkType.None) continue;
//            sql.Append($"\n{temp.TableType.ToLabel()} {DbEmphasis(temp.TableName!)} {temp.Alias} ON {temp.Fragment}");
//        }
//        if (where != null)
//            sql.Append($"\nWHERE {where}");

//        if (groupBy != null)
//        {
//            groupBy.RemoveLastComma();
//            sql.Append($"\nGROUP BY {groupBy}");
//        }

//        if (orderBy != null)
//        {
//            orderBy.RemoveLastComma();
//            sql.Append($"\nORDER BY {orderBy} {(isAsc ? "ASC" : "DESC")}");
//        }

//        if (index * size > 0)
//        {
//            //分页处理
//            DbPaging(sql, null, isAsc, null, index, size);
//        }

//        return sql.ToString();
//    }

//    public string DbEmphasis(string columnName) => $"[{columnName}]";

//    public virtual void DbPaging(StringBuilder sql, SqlFragment? orderby, bool isAsc, UnitCell? defaultCell, int index, int size)
//    {
//        throw new NotImplementedException();
//    }

//    public string DbStringConvert(string content) => $"CASE({content} as VARCHAR)";

//    public string GetColumnEmphasis(bool isLeft) => isLeft ? "[" : "]";

//    public string GetPrefix() => "@";
//}
