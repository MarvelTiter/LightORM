namespace LightORMTest.Sqlite;

[TestClass]
public class CreateTableTest : LightORMTest.CreateTableTest
{
    public override DbBaseType DbType => DbBaseType.Sqlite;
    public override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlite(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
