using System;
using System.Collections.Generic;
using System.Text;

namespace LightORMTest.Oracle.ScopedTest;

[TestClass]
public class TransactionTest: LightORMTest.ScopedTest.TransactionTest
{
    public override DbBaseType DbType => DbBaseType.Oracle;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseOracle(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
