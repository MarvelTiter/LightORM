namespace LightORMTest.SqlServer.ResultTest;

[TestClass]
public class ExecutionTest : LightORMTest.ResultTest.ExecutionTest
{
    public override DbBaseType DbType => DbBaseType.SqlServer;

    protected override void Configura(IExpressionContextSetup option)
    {
        // 不指定版本：由后台探测决定（真库）。需固定版本用 o.SpecificVersion，需跳过探测用 o.DetectVersion = false。
        option.UseSqlServer(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
