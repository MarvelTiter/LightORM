﻿namespace LightORMTest.PostgreSQL;

[TestClass]
public class CreateTableTest : LightORMTest.CreateTableTest
{
    public override DbBaseType DbType => DbBaseType.PostgreSQL;
    public override void Configura(IExpressionContextSetup option)
    {
        option.UsePostgreSQL(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }
}
