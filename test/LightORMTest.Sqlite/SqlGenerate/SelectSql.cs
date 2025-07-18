﻿namespace LightORMTest.Sqlite.SqlGenerate;

[TestClass]
public class SelectSql : LightORMTest.SqlGenerate.SelectSql
{
    public override DbBaseType DbType => DbBaseType.Sqlite;
    public override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlite(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
