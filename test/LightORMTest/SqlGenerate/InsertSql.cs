using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest.SqlGenerate;

[TestClass]
public class InsertSql : TestBase
{
    const string UID = "TEST_USER";
    [TestMethod]
    public void InsertEntity()
    {
        int hour = DateTime.Now.Hour;
        User? u = new()
        {
            UserId = UID,
            Age = hour,
            LastLogin = DateTime.Now,
        };
        var sql = Db.Insert(u).ToSql();
        Console.WriteLine(sql);
        var result = """
                INSERT INTO `USER` 
                (`USER_ID`, `AGE`, `LAST_LOGIN`, `IS_LOCK`)
                VALUES
                (@UserId, @Age, @LastLogin, @IsLock)
                """;
        Assert.IsTrue(result == sql);
    }
}
