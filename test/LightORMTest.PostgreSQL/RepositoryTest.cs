namespace LightORMTest.PostgreSQL;

[TestClass]
public class RepositoryTest : LightORMTest.RepositoryTest
{
    public override DbBaseType DbType => DbBaseType.PostgreSQL;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UsePostgreSQL(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
