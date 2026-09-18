namespace LightORMTest.SqlServer;

[TestClass]
public class CreateTableTest : LightORMTest.CreateTableTest
{
    public override DbBaseType DbType => DbBaseType.SqlServer;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlServer(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
