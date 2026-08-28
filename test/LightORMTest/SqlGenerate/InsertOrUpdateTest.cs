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
        var sql = Db.Insert(user).OrUpdate().ToSqlWithParameters();
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void InsertOrUpdateBatch()
    {
        var user = new User()
        {
            UserId = "testUpsert1",
            Age = 18,
            UserName = "测试1",
        };
        var user1 = new User()
        {
            UserId = "testUpsert2",
            Age = 19,
            UserName = "测试2",
        };
        var user2 = new User()
        {
            UserId = "testUpsert3",
            Age = 20,
            UserName = "测试3",
        };
        var sql = Db.Insert(user, user1, user2).OrUpdate().ToSqlWithParameters();
        Console.WriteLine(sql);
    }
}
