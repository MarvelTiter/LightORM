
namespace LightORMTest.Dameng.ResultTest;

[TestClass]
public class JsonTest : LightORMTest.ResultTest.JsonTest
{
    public override DbBaseType DbType => DbBaseType.Dameng;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseDameng(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}