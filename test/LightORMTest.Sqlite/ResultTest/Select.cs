using System;
using System.Collections.Generic;
using System.Text;

namespace LightORMTest.Sqlite.ResultTest;

[TestClass]
public class Select : LightORMTest.ResultTest.Select
{
    public override DbBaseType DbType => DbBaseType.Sqlite;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlite(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
