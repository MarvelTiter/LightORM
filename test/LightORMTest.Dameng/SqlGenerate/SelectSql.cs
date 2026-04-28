using LightORMTest.Dameng;

namespace LightORMTest.Dameng.SqlGenerate;

[TestClass]
public class SelectSql : LightORMTest.SqlGenerate.SelectSql
{
    public override DbBaseType DbType => DatabaseType.Dameng;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseDameng(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
