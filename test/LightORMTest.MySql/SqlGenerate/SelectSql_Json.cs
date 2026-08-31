namespace LightORMTest.MySql.SqlGenerate;

[TestClass]
public class SelectSql_Json : LightORMTest.SqlGenerate.SelectSql_Json
{
    public override DbBaseType DbType => DbBaseType.MySql;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseMySql(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
