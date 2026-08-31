namespace LightORMTest.Dameng.SqlGenerate;

[TestClass]
public class DeleteSql:LightORMTest.SqlGenerate.DeleteSql
{
    public override DbBaseType DbType => DatabaseType.Dameng;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseDameng(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
