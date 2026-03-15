namespace LightORMTest.Sqlite.SqlGenerate;

[TestClass]
public class InsertSql_Json : LightORMTest.SqlGenerate.InsertSql_Json
{
    public override DbBaseType DbType => DbBaseType.Sqlite;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlite(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
