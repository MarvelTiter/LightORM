namespace LightORMTest.PostgreSQL.SqlGenerate;

[TestClass]
public class SelectSql : LightORMTest.SqlGenerate.SelectSql
{
    public override DbBaseType DbType => DbBaseType.PostgreSQL;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UsePostgreSQL(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
