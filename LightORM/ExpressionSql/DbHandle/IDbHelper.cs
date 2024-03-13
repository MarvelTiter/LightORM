using LightORM.ExpressionSql;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LightORM.ExpressionSql.DbHandle;

internal interface IDbHelper
{
    string GetPrefix();
    string GetColumnEmphasis(bool isLeft);
    /// <summary>
    /// 防止与关键字冲突
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    string DbEmphasis(string columnName);
    string DbStringConvert(string content);
    //void DbPaging(SqlContext context, IList<UnitCell> orderby, StringBuilder sql, int index, int size);
    string BuildSelectSql(SqlContext context, SqlFragment? select, bool distanct, SqlFragment? where, SqlFragment? groupBy, SqlFragment? orderBy, bool isAsc, int index, int size);
}
