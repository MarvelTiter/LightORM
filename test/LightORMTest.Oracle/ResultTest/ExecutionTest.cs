using System;
using System.Collections.Generic;
using System.Text;

namespace LightORMTest.Oracle.ResultTest;

[TestClass]
public class ExecutionTest : LightORMTest.ResultTest.ExecutionTest
{
    public override DbBaseType DbType => DbBaseType.Oracle;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseOracle(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
