using System;
using System.Collections.Generic;
using System.Text;

namespace LightORMTest.PostgreSQL.SqlGenerate;

[TestClass]
public class SelectSql_Json : LightORMTest.SqlGenerate.SelectSql_Json
{
    public override DbBaseType DbType => DbBaseType.PostgreSQL;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UsePostgreSQL(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
