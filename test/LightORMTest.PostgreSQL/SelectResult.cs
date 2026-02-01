using System;
using System.Collections.Generic;
using System.Text;

namespace LightORMTest.PostgreSQL;

[TestClass]
public class SelectResult : LightORMTest.ResultTest.Select
{
    public override DbBaseType DbType => DbBaseType.PostgreSQL;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UsePostgreSQL(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
