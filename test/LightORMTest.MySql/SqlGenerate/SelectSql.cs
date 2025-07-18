﻿namespace LightORMTest.MySql.SqlGenerate;

[TestClass]
public class SelectSql : LightORMTest.SqlGenerate.SelectSql
{
    public override DbBaseType DbType => DbBaseType.MySql;
    public override void Configura(IExpressionContextSetup option)
    {
        option.UseMySql(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
