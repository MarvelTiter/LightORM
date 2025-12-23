namespace LightORMTest.MySql;

[TestClass]
public class BulkCopyTest : LightORMTest.BulkCopyTest
{
    public override DbBaseType DbType => DbBaseType.MySql;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseMySql(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
