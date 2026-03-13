using System;
using System.Collections.Generic;
using System.Text;

namespace LightORMTest.Sqlite.SqlGenerate;

[TestClass]
public class SelectSql_Json: LightORMTest.SqlGenerate.SelectSql_Json
{
    public override DbBaseType DbType => DbBaseType.Sqlite;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlite(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
