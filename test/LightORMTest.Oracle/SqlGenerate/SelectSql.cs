namespace LightORMTest.Oracle.SqlGenerate;

[TestClass]
public class SelectSql : LightORMTest.SqlGenerate.SelectSql
{
    public override DbBaseType DbType => DbBaseType.Oracle;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseOracle(o =>
        {
            o.MasterConnectionString = ConnectString.Value;
            o.UseIdentifierQuote(false);
        });
        option.UseInterceptor<LightOrmAop>();
    }

    protected override void ConfiguraSqlResults(Dictionary<string, string> results)
    {
        results[nameof(Select_One_Table)] = """
                                            SELECT a.PRODUCT_ID AS ProductId, a.PRODUCT_NAME AS ProductName
                                            FROM PRODUCTS a
                                            WHERE (a.MODIFY_TIME > :Now_0_0)
                                            """;
    }
}
