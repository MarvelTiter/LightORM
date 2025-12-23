namespace LightORMTest.Oracle;

[TestClass]
public class BulkCopyTest : LightORMTest.BulkCopyTest
{
    public override DbBaseType DbType => DbBaseType.Oracle;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseOracle(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
