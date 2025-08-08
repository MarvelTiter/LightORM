using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest.PostgreSQL.SqlGenerate;

[TestClass]
public class UpdateSql: LightORMTest.SqlGenerate.UpdateSql
{
    public override DbBaseType DbType => DbBaseType.PostgreSQL;
    public override void Configura(IExpressionContextSetup option)
    {
        option.UsePostgreSQL(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
