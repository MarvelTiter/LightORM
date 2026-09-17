using LightORM;
using LightORM.Interfaces;
using LightORM.Providers.KingbaseES.Extensions;

namespace LightORMTest.KingbaseES.ResultTest;

[TestClass]
public class ExecutionTest : LightORMTest.ResultTest.ExecutionTest
{
    public override DbBaseType DbType => LightORM.Providers.KingbaseES.KingbaseESProvider.KingbaseEs;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseKingbaseES(static option =>
        {
            option.MasterConnectionString = ConnectString.Value;
            
        });
        option.UseInterceptor<LightOrmAop>();
    }
}
