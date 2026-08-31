namespace LightORMTest.Oracle.SqlGenerate;

[TestClass]
public class DeleteSql : LightORMTest.SqlGenerate.DeleteSql
{
    public override DbBaseType DbType => DbBaseType.Oracle;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseOracle(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
