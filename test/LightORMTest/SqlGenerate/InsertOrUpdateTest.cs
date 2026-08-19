using System;
using System.Collections.Generic;
using System.Text;

namespace LightORMTest.SqlGenerate;

public class InsertOrUpdateTest : TestBase
{
    [TestMethod]
    public void InsertOrUpdateSingle()
    {
        var user = new User()
        {
            UserId = "testUpsert",
            Age = 18,
            UserName = "测试",
        };
        var sql = Db.Insert(user).OrUpdate().NoQuoteIdentifiers().ToSqlWithParameters();
        Console.WriteLine(sql);
    }
}
