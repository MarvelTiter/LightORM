
namespace LightORMTest.Oracle.ResultTest;

[TestClass]
public class JsonTest : LightORMTest.ResultTest.JsonTest
{
    public override DbBaseType DbType => DbBaseType.Oracle;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseOracle(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}