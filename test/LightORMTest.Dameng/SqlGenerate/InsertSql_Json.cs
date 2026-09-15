namespace LightORMTest.Dameng.SqlGenerate;

[TestClass]
public class InsertSql_Json : LightORMTest.SqlGenerate.InsertSql_Json
{
    public override DbBaseType DbType => DatabaseType.Dameng;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseDameng(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
