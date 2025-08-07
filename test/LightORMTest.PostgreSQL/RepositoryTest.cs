using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest.PostgreSQL;

[TestClass]
public class RepositoryTest : LightORMTest.RepositoryTest
{
    public override DbBaseType DbType => DbBaseType.PostgreSQL;
    public override void Configura(IExpressionContextSetup option)
    {
        option.UsePostgreSQL(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
