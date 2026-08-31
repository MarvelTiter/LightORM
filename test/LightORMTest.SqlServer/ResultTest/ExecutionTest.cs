namespace LightORMTest.SqlServer.ResultTest;

[TestClass]
public class ExecutionTest : LightORMTest.ResultTest.ExecutionTest
{
    public override DbBaseType DbType => DbBaseType.SqlServer;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlServer(LightORM.Providers.SqlServer.SqlServerVersion.V1, ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
