
namespace LightORMTest.MySql.ResultTest;

[TestClass]
public class JsonTest : LightORMTest.ResultTest.JsonTest
{
    public override DbBaseType DbType => DbBaseType.MySql;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseMySql(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}