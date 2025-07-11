namespace LightORMTest.PostgreSQL;

[TestClass]
public class DbMethodTest : LightORMTest.DbMethodTest
{
    public override DbBaseType DbType => DbBaseType.PostgreSQL;
    public override void Configura(IExpressionContextSetup option)
    {
        option.UsePostgreSQL("Host=localhost;Port=5432;Database=testdb;Username=postgres;Password=123456;");
        option.UseInterceptor<LightOrmAop>();
    }
}
