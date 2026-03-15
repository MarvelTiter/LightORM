using System;
using System.Collections.Generic;
using System.Text;

namespace LightORMTest.SqlServer.SqlGenerate;

[TestClass]
public class SelectSql_Json : LightORMTest.SqlGenerate.SelectSql_Json
{
    public override DbBaseType DbType => DbBaseType.SqlServer;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlServer(LightORM.Providers.SqlServer.SqlServerVersion.V1, ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
