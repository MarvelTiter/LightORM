using System;
using System.Collections.Generic;
using System.Text;

namespace LightORMTest.Oracle.SqlGenerate;

[TestClass]
public class SelectSql_Json : LightORMTest.SqlGenerate.SelectSql_Json
{
    public override DbBaseType DbType => DbBaseType.Oracle;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseOracle(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
