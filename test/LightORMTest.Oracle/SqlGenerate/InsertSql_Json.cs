namespace LightORMTest.Oracle.SqlGenerate;

[TestClass]
public class InsertSql_Json : LightORMTest.SqlGenerate.InsertSql_Json
{
    public override DbBaseType DbType => DbBaseType.Oracle;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseOracle(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
