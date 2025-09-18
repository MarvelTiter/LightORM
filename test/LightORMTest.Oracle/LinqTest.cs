using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest.Oracle;

[TestClass]
public class LinqTest : LightORMTest.LinqTest
{
    public override DbBaseType DbType => DbBaseType.Oracle;
    public override void Configura(IExpressionContextSetup option)
    {
        option.UseOracle(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}