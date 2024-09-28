﻿//using System.Collections.Generic;
using System.Text;
using LightORM.Builder;

namespace LightORM.ExpressionSql.DbHandle;
internal class OracleDb : IDbHelper
{
    public void Paging(SelectBuilder builder, StringBuilder sql)
    {
        sql.Insert(0, $" SELECT ROWNUM as ROWNO, SubMax.* FROM ({SqlBuilder.N} ");
        sql.AppendLine(" ) SubMax WHERE ROWNUM <= ");
        sql.Append(builder.PageIndex * builder.PageSize);
        sql.Insert(0, $" SELECT * FROM ({SqlBuilder.N} ");
        sql.AppendLine(" ) SubMin WHERE SubMin.ROWNO > ");
        sql.Append((builder.PageIndex - 1) * builder.PageSize);
    }

    public string ReturnIdentitySql()
    {
        throw new NotImplementedException();
    }
}
