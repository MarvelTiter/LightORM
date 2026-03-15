namespace LightORMTest.PostgreSQL.SqlGenerate;

[TestClass]
public class UpdateSql_Json : LightORMTest.SqlGenerate.UpdateSql_Json
{
    public override DbBaseType DbType => DbBaseType.PostgreSQL;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UsePostgreSQL(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
