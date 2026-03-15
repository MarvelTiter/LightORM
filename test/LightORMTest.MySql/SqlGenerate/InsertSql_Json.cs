namespace LightORMTest.MySql.SqlGenerate;

[TestClass]
public class InsertSql_Json : LightORMTest.SqlGenerate.InsertSql_Json
{
    public override DbBaseType DbType => DbBaseType.MySql;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseMySql(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
