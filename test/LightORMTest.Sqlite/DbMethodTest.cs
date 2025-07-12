namespace LightORMTest.Sqlite;

[TestClass]
public class DbMethodTest : LightORMTest.DbMethodTest
{
    public override DbBaseType DbType => DbBaseType.Sqlite;
    public override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlite(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
