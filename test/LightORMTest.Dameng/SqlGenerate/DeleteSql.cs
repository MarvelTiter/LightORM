using LightORMTest.Dameng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest.Dameng.SqlGenerate;

[TestClass]
public class DeleteSql:LightORMTest.SqlGenerate.DeleteSql
{
    public override DbBaseType DbType => DbBaseType.Dameng;
    public override void Configura(IExpressionContextSetup option)
    {
        option.UseDameng(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
