using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest.SqlGenerate;

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
        //var result = """
        //        INSERT INTO `USER` 
        //        (`USER_ID`, `AGE`, `LAST_LOGIN`, `IS_LOCK`)
        //        VALUES
        //        (@UserId, @Age, @LastLogin, @IsLock)
        //        """;
        //Assert.IsTrue(result == sql);
    }

    [TestMethod]
    public void Insert_Flat_Entity()
    {
        var sql = Db.Insert(new UserFlat()).ToSql();
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void InsertEntity_AutoIncrement()
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
        //var result = """
        //        INSERT INTO `USER` 
        //        (`USER_ID`, `AGE`, `LAST_LOGIN`, `IS_LOCK`)
        //        VALUES
        //        (@UserId, @Age, @LastLogin, @IsLock)
        //        """;
        //Assert.IsTrue(result == sql);
    }

    [TestMethod]
    public void Select_Then_Insert()
    {
        int hour = DateTime.Now.Hour;
        User? u = new()
        {
            UserId = UID,
            Age = hour,
            LastLogin = DateTime.Now,
        };
        var sql = Db.Select<User>()
            .Where(x => x.UserId == UID)
            .SelectColumns(x => new { x.UserId, x.UserName })
            .Insert<UserFlat>(u => new { u.UserId, u.UserName })
            .ToSql();
        Console.WriteLine(sql);
        //var result = """
        //        INSERT INTO `USER` 
        //        (`USER_ID`, `AGE`, `LAST_LOGIN`, `IS_LOCK`)
        //        VALUES
        //        (@UserId, @Age, @LastLogin, @IsLock)
        //        """;
        //Assert.IsTrue(result == sql);
    }

    [TestMethod]
    public void InsertRange()
    {
        var scoped = Db.CreateMainDbScoped();
        try
        {
            scoped.BeginTransaction();
            scoped.Delete<User>().FullDelete(true).Execute();
            //Db.Insert<User>().InsertEach([new User { UserId = "TEST_USER1", Age = 20, LastLogin = DateTime.Now },
            //    new User { UserId = "TEST_USER2", Age = 25, LastLogin = DateTime.Now }
            //]);
            scoped.Insert([
                new User { UserId = "TEST_USER1", Age = 20, LastLogin = DateTime.Now },
                new User { UserId = "TEST_USER2", Age = 25, LastLogin = DateTime.Now }
            ]).Execute();
            scoped.CommitTransaction();
        }
        catch (Exception)
        {
            scoped.RollbackTransaction();
            throw;
        }
    }
}
