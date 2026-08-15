using System;
using System.Collections.Generic;
using System.Text;

namespace LightORMTest.PostgreSQL.ResultTest;

[TestClass]
public class ExecutionTest : LightORMTest.ResultTest.ExecutionTest
{
    public override DbBaseType DbType => DbBaseType.PostgreSQL;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UsePostgreSQL(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
