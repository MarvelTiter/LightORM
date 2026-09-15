namespace LightORMTest.SqlServer.ResultTest;

[TestClass]
public class ExecutionTest : LightORMTest.ResultTest.ExecutionTest
{
    public override DbBaseType DbType => DbBaseType.SqlServer;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlServer(LightORM.Providers.SqlServer.SqlServerVersion.Over2017, ConnectString.Value);
        //option.UseSqlServer(LightORM.Providers.SqlServer.SqlServerVersion.V1, o =>
        //{
        //    o.MasterConnectionString = ConnectString.Value;
        //    o.ConfiguraSqlServer(t =>
        //    {
        //        t.UseUnicodeString = false;
        //    });
        //});
        option.UseInterceptor<LightOrmAop>();
    }
}
