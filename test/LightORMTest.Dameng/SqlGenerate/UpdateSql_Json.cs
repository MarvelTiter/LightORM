namespace LightORMTest.Dameng.SqlGenerate;

[TestClass]
public class UpdateSql_Json : LightORMTest.SqlGenerate.UpdateSql_Json
{
    public override DbBaseType DbType => DatabaseType.Dameng;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseDameng(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
