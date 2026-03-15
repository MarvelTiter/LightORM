namespace LightORMTest.Sqlite.ResultTest;

[TestClass]
public class JsonTest : LightORMTest.ResultTest.JsonTest
{
    public override DbBaseType DbType => DbBaseType.Sqlite;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlite(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}