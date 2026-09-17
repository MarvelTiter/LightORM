namespace LightORMTest.Oracle.ResultTest;

[TestClass]
public class ExecutionTest : LightORMTest.ResultTest.ExecutionTest
{
    public override DbBaseType DbType => DbBaseType.Oracle;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseOracle(static option =>
        {
            option.MasterConnectionString = ConnectString.Value;
            option.ConfigureOracle(o =>
            {
                o.UseUnicodeString = false;
            });
        });
        option.UseInterceptor<LightOrmAop>();
    }
}
