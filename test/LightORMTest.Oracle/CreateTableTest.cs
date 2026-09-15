namespace LightORMTest.Oracle;

[TestClass]
public class CreateTableTest : LightORMTest.CreateTableTest
{
    public override DbBaseType DbType => DbBaseType.Oracle;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseOracle(option =>
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
