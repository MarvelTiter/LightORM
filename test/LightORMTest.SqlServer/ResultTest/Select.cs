using LightORMTest.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace LightORMTest.SqlServer.ResultTest;

[TestClass]
public class Select : LightORMTest.ResultTest.Select
{
    public override DbBaseType DbType => DbBaseType.SqlServer;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlServer(LightORM.Providers.SqlServer.SqlServerVersion.V1, ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }

    [TestMethod]
    public void TestBooleanInWhere()
    {
      var list =  Db.Select<User>().Where(u => u.IsLock == false).ToList();
        foreach (var item in list)
        {
            Console.WriteLine($"{item.UserName} -> {item.IsLock}");
        }
    }
}
