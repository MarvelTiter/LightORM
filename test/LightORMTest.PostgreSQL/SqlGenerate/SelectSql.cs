namespace LightORMTest.PostgreSQL.SqlGenerate;

[TestClass]
public class SelectSql : LightORMTest.SqlGenerate.SelectSql
{
    public override DbBaseType DbType => DbBaseType.PostgreSQL;
    public override void Configura(IExpressionContextSetup option)
    {
        option.UsePostgreSQL("Host=localhost;Port=5432;Database=testdb;Username=postgres;Password=123456;");
        option.UseInterceptor<LightOrmAop>();
    }
}
