namespace LightORMTest.PostgreSQL.SqlGenerate;

[TestClass]
public class DeleteSql:LightORMTest.SqlGenerate.DeleteSql
{
    public override DbBaseType DbType => DbBaseType.PostgreSQL;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UsePostgreSQL(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
