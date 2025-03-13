using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.SqlTest;

[TestClass]
public class SelectResult : TestBase
{
    [TestMethod]
    public void ToListResult()
    {
        var list = Db.Select<User>().ToList();
        foreach (var item in list)
        {
            Console.WriteLine(item.UserId);
            if (item.UserId == "admin")
            {
                Assert.IsTrue(true);
            }
        }
    }

    [TestMethod]
    public void AnonymousResult()
    {
        var list = Db.Select<User>().ToList(u => new { u.UserId, u.UserName });
        foreach (var item in list)
        {
            Console.WriteLine(item.UserId);
            if (item.UserId == "admin")
            {
                Assert.IsTrue(true);
            }
        }
    }

    class SimpleUser
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
    }
    [TestMethod]
    public void AcceptAnonymousResult()
    {
        var list = Db.Select<User>()
            .AsTable(u => new { u.UserId, u.UserName })
            .ToList<SimpleUser>();
        foreach (var item in list)
        {
            Console.WriteLine(item.UserId);
            if (item.UserId == "admin")
            {
                Assert.IsTrue(true);
            }
        }
    }
    class SimpleUser2
    {
        public string? USER_ID { get; set; }
        public string? USER_NAME { get; set; }
    }
    [TestMethod]
    public void AcceptOtherTypeResult()
    {
        var list = Db.Select<User>()
            .ToList<SimpleUser2>();
        foreach (var item in list)
        {
            Console.WriteLine(item.USER_ID);
            if (item.USER_ID == "admin")
            {
                Assert.IsTrue(true);
            }
        }
    }
}
