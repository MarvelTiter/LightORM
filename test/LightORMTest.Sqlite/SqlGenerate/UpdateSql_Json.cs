namespace LightORMTest.Sqlite.SqlGenerate;

[TestClass]
public class UpdateSql_Json : LightORMTest.SqlGenerate.UpdateSql_Json
{
    public override DbBaseType DbType => DbBaseType.Sqlite;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlite(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
