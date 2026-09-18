namespace LightORMTest.Oracle.SqlGenerate;

[TestClass]
public class InsertOrUpdateTest : LightORMTest.SqlGenerate.InsertOrUpdateTest
{
    public override DbBaseType DbType => DbBaseType.Oracle;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseOracle(c =>
        {
            c.MasterConnectionString = ConnectString.Value;
            //c.ConfigureOracle(t =>
            //{
            //    // 21c：自增列可用 GENERATED ALWAYS AS IDENTITY（等效于旧的 OverVersion = true）
            //    t.SpecificVersion = new Version(21, 3, 0, 0);
            //});
        });
        option.UseInterceptor<LightOrmAop>();
    }
}
