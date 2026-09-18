namespace LightORMTest.Oracle.SqlGenerate;

[TestClass]
public class InsertSql : LightORMTest.SqlGenerate.InsertSql
{
    public override DbBaseType DbType => DbBaseType.Oracle;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseOracle(static c =>
        {
            c.MasterConnectionString = ConnectString.Value;
            c.ConfigureOracle(t =>
            {
                t.DetectVersion = false;
            });
        });
        option.UseInterceptor<LightOrmAop>();
    }
}
