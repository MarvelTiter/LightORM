namespace LightORMTest.Dameng;

[TestClass]
public class DbMethodTest : LightORMTest.DbMethodTest
{
    public override DbBaseType DbType => DbBaseType.Dameng;
    public override void Configura(IExpressionContextSetup option)
    {
        option.UseDameng(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
