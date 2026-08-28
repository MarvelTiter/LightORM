namespace LightORMTest.SqlServer.SqlGenerate;

[TestClass]
public class InsertOrUpdateTest : LightORMTest.SqlGenerate.InsertOrUpdateTest
{
    public override DbBaseType DbType => DbBaseType.SqlServer;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlServer(LightORM.Providers.SqlServer.SqlServerVersion.V1, ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}