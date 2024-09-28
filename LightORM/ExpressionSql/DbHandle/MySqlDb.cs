using LightORM.Builder;
using LightORM.ExpressionSql;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace LightORM.ExpressionSql.DbHandle;

internal class MySqlDb : IDbHelper
{
    public void Paging(SelectBuilder builder, StringBuilder sql)
    {
        sql.AppendLine($"LIMIT {(builder.PageIndex - 1) * builder.PageSize}, {builder.PageSize}");
    }

    public string ReturnIdentitySql() => "SELECT @@IDENTITY";
}