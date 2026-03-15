namespace LightORMTest.PostgreSQL.SqlGenerate;

[TestClass]
public class InsertSql_Json : LightORMTest.SqlGenerate.InsertSql_Json
{
    public override DbBaseType DbType => DbBaseType.PostgreSQL;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UsePostgreSQL(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
