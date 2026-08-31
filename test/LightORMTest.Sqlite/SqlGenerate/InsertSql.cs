namespace LightORMTest.Sqlite.SqlGenerate;

[TestClass]
public class InsertSql : LightORMTest.SqlGenerate.InsertSql
{
    public override DbBaseType DbType => DbBaseType.Sqlite;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlite(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
