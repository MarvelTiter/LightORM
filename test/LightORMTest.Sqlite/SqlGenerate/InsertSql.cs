using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest.Sqlite.SqlGenerate;

[TestClass]
public class InsertSql : LightORMTest.SqlGenerate.InsertSql
{
    public override DbBaseType DbType => DbBaseType.Sqlite;
    public override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlite(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
