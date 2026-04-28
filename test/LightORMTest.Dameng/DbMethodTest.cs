namespace LightORMTest.Dameng;

[TestClass]
public class DbMethodTest : LightORMTest.DbMethodTest
{
    public override DbBaseType DbType => DatabaseType.Dameng;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseDameng(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
