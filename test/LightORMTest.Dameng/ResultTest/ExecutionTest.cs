using System;
using System.Collections.Generic;
using System.Text;

namespace LightORMTest.Dameng.ResultTest;

[TestClass]
public class ExecutionTest : LightORMTest.ResultTest.ExecutionTest
{
    public override DbBaseType DbType => DatabaseType.Dameng;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseDameng(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
