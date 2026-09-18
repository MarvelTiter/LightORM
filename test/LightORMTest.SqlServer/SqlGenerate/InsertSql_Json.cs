namespace LightORMTest.SqlServer.SqlGenerate;

[TestClass]
public class InsertSql_Json : LightORMTest.SqlGenerate.InsertSql_Json
{
    public override DbBaseType DbType => DbBaseType.SqlServer;

    protected override void Configura(IExpressionContextSetup option)
    {
        // 纯 SQL 生成，不连库：关闭版本探测，按完整功能生成。
        option.UseSqlServer(set =>
        {
            set.MasterConnectionString = ConnectString.Value;
            set.ConfiguraSqlServer(o => o.DetectVersion = false);
        });
        option.UseInterceptor<LightOrmAop>();
    }
}
