using LightORM.Builder;
using LightORM.ExpressionSql;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LightORM.ExpressionSql.DbHandle;

internal interface IDbHelper
{
    void Paging(SelectBuilder builder, StringBuilder sql);
    string ReturnIdentitySql();
}
