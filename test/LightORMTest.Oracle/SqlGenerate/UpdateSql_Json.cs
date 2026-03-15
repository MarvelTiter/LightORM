namespace LightORMTest.Oracle.SqlGenerate;

[TestClass]
public class UpdateSql_Json : LightORMTest.SqlGenerate.UpdateSql_Json
{
    public override DbBaseType DbType => DbBaseType.Oracle;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseOracle(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
