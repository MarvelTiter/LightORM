using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest.ResultTest;

public class Select : TestBase
{
    private static async Task DoSomethingWithTempUserDataAsync(IExpressionContext db, Func<Task> action)
    {
        db.Delete<User>().FullDelete(true).Execute();
        db.Delete<UserRole>().FullDelete(true).Execute();
        db.Delete<Role>().FullDelete(true).Execute();
        await db.Insert<User>().InsertEachAsync([new User()
        {
            UserId = "test01",
            UserName = "Test1",
            Age = 11,
            IsLock = false,
            Password = "helloworld",
        }, new User()
        {
            UserId = "test02",
            UserName = "Test2",
            Age = 9,
            IsLock = true,
            Password = "helloworld",
        }, new User()
        {
            UserId = "test03",
            UserName = "Test3",
            Age = 8,
            IsLock = false,
            Password = "helloworld",
        }, new User()
        {
            UserId = "test04",
            UserName = "Test4",
            Age = 12,
            IsLock = true,
            Password = "helloworld",
        }]);
        await db.Insert<UserRole>().InsertEachAsync([new UserRole()
        {
            UserId = "test01",
            RoleId = "Admin"
        }, new UserRole()
        {
            UserId = "test02",
            RoleId = "SuperAdmin"
        }, new UserRole()
        {
            UserId = "test03",
            RoleId = "Admin"
        }, new UserRole()
        {
            UserId = "test03",
            RoleId = "SuperAdmin"
        }]);
        await db.Insert<Role>().InsertEachAsync([new Role()
        {
            RoleId = "Admin",
            RoleName = "管理员"
        }, new Role()
        {
            RoleId = "SuperAdmin",
            RoleName = "超级管理员"
        }]);
        await action();
        db.Delete<User>().FullDelete(true).Execute();
        db.Delete<UserRole>().FullDelete(true).Execute();
        db.Delete<Role>().FullDelete(true).Execute();
    }
    [TestMethod]
    public async Task Select_ToListAsync()
    {
        await DoSomethingWithTempUserDataAsync(Db, async () =>
        {
            var list = await Db.Select<User>().ToListAsync();
            foreach (var item in list)
            {
                Console.WriteLine($"{item.UserId} - {item.UserName}");
            }
        });
    }

    [TestMethod]
    public async Task Select_ToListAsync_With_Count()
    {
        await DoSomethingWithTempUserDataAsync(Db, async () =>
        {
            int age = 10;
            var list = await Db.Select<User>()
                .Where(u => u.Age < age)
                .Count(out var total)
                .ToListAsync();
            Assert.IsTrue(total == 2);
            Console.WriteLine($"Total: {total}");
            foreach (var item in list)
            {
                Console.WriteLine($"{item.UserId} - {item.UserName}");
            }
        });
    }

    [TestMethod]
    public async Task Select_ToEnumerableAsync()
    {
        await DoSomethingWithTempUserDataAsync(Db, async () =>
        {
            var list = Db.Select<User>().ToEnumerableAsync();
            await foreach (var item in list)
            {
                Console.WriteLine($"{item.UserId} - {item.UserName}");
            }
        });
    }

    [TestMethod]
    public async Task Select_Include_IncludeWhere_Result()
    {
        await DoSomethingWithTempUserDataAsync(Db, async () =>
        {
            var result = await Db.Select<User>()
            .Include(u => u.UserRoles)
            .Where(u => u.UserId == "test01")
            .FirstAsync();
            Assert.IsFalse(result is null);
            Assert.IsFalse(result.UserRoles is null);
            Assert.IsTrue(result.UserRoles.Count() == 1);

            result = await Db.Select<User>()
                .Include(u => u.UserRoles.Where(r => r.RoleId.StartsWith("Ad")))
                .Where(u => u.UserId == "test03")
                .FirstAsync();

            Assert.IsFalse(result is null);
            Assert.IsFalse(result.UserRoles is null);
            Assert.IsTrue(result.UserRoles.Count() == 1);
            Assert.IsTrue(result.UserRoles.FirstOrDefault()!.RoleId == "Admin");

            result = await Db.Select<User>()
                .Include(u => u.UserRoles.Where(r => r.RoleId.StartsWith("Su")))
                .Where(u => u.UserId == "test03")
                .FirstAsync();
            Assert.IsFalse(result is null);
            Assert.IsFalse(result.UserRoles is null);
            Assert.IsTrue(result.UserRoles.Count() == 1);
            Assert.IsTrue(result.UserRoles.FirstOrDefault()!.RoleId == "SuperAdmin");
        });
    }

    [TestMethod]
    public async Task SelectSingleStringColumn()
    {
        await DoSomethingWithTempUserDataAsync(Db, async () =>
        {
            var list = await Db.Select<User>().ToListAsync(u => u.UserName);
            Assert.IsTrue(list.Count == 4);
            Assert.IsTrue(list[0] == "Test1");
            Assert.IsTrue(list[1] == "Test2");
        });
    }

    [TestMethod]
    public async Task SelectSingleIntColumn()
    {
        await DoSomethingWithTempUserDataAsync(Db, async () =>
        {
            var list = await Db.Select<User>().ToListAsync(u => u.Age);
            Assert.IsTrue(list.Count == 4);
            Assert.IsTrue(list[0] == 11);
            Assert.IsTrue(list[1] == 9);
        });
    }

    [TestMethod]
    public async Task SelectAnonymousResult()
    {
        await DoSomethingWithTempUserDataAsync(Db, async () =>
        {
            var list = await Db.Select<User>().ToListAsync(u => new { u.UserId, u.UserName, u.Age });
            Assert.IsTrue(list.Count == 4);
            Assert.IsTrue(list[0].UserId == "test01");
            Assert.IsTrue(list[0].UserName == "Test1");
            Assert.IsTrue(list[0].Age == 11);
        });
    }

    [TestMethod]
    public async Task SelectGroupResult()
    {
        await DoSomethingWithTempUserDataAsync(Db, async () =>
        {
            var list = await Db.Select<User>()
                .GroupBy(u => u.IsLock)
                .ToListAsync(g => new
                {
                    IsLock = g.Group,
                    Total = g.Count(),
                    AvgAge = g.Average(g.Tables.Age),
                    MaxAge = g.Max(g.Tables.Age),
                    MinAge = g.Min(g.Tables.Age),
                });
            Assert.IsTrue(list.Count == 2);
            Assert.IsTrue(list.Any(r => r.IsLock == true && r.Total == 2 && r.AvgAge == 10.5 && r.MaxAge == 12 && r.MinAge == 9));
            Assert.IsTrue(list.Any(r => r.IsLock == false && r.Total == 2 && r.AvgAge == 9.5 && r.MaxAge == 11 && r.MinAge == 8));
        });
    }
}
