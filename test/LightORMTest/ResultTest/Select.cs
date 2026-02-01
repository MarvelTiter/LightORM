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
            Sign = SignType.Svip
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
            Assert.AreEqual(2, total);
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
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.UserRoles);
            Assert.AreEqual(1, result.UserRoles.Count());

            result = await Db.Select<User>()
                .Include(u => u.UserRoles.Where(r => r.RoleId.StartsWith("Ad")))
                .Where(u => u.UserId == "test03")
                .FirstAsync();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.UserRoles);
            Assert.AreEqual(1, result.UserRoles.Count());
            Assert.AreEqual("Admin", result.UserRoles.FirstOrDefault()!.RoleId);

            result = await Db.Select<User>()
                .Include(u => u.UserRoles.Where(r => r.RoleId.StartsWith("Su")))
                .Where(u => u.UserId == "test03")
                .FirstAsync();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.UserRoles);
            Assert.AreEqual(1, result.UserRoles.Count());
            Assert.AreEqual("SuperAdmin", result.UserRoles.FirstOrDefault()!.RoleId);
        });
    }

    [TestMethod]
    public async Task SelectSingleStringColumn()
    {
        await DoSomethingWithTempUserDataAsync(Db, async () =>
        {
            var list = await Db.Select<User>().ToListAsync(u => u.UserName);
            Assert.HasCount(4, list);
            Assert.AreEqual("Test1", list[0]);
            Assert.AreEqual("Test2", list[1]);
        });
    }

    [TestMethod]
    public async Task SelectSingleIntColumn()
    {
        await DoSomethingWithTempUserDataAsync(Db, async () =>
        {
            var list = await Db.Select<User>().ToListAsync(u => u.Age);
            Assert.HasCount(4, list);
            Assert.AreEqual(11, list[0]);
            Assert.AreEqual(9, list[1]);
        });
    }

    [TestMethod]
    public async Task SelectAnonymousResult()
    {
        await DoSomethingWithTempUserDataAsync(Db, async () =>
        {
            var list = await Db.Select<User>().ToListAsync(u => new { u.UserId, u.UserName, u.Age });
            Assert.HasCount(4, list);
            Assert.AreEqual("test01", list[0].UserId);
            Assert.AreEqual("Test1", list[0].UserName);
            Assert.AreEqual(11, list[0].Age);
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
            Assert.HasCount(2, list);
            Assert.IsTrue(list.Any(r => r.IsLock == true && r.Total == 2 && r.AvgAge == 10.5 && r.MaxAge == 12 && r.MinAge == 9));
            Assert.IsTrue(list.Any(r => r.IsLock == false && r.Total == 2 && r.AvgAge == 9.5 && r.MaxAge == 11 && r.MinAge == 8));
        });
    }


    class UserDto
    {
        public string? UserId { get; set; }
        public SignType Sign { get; set; }
    }
    [TestMethod]
    public async Task SelectProjection()
    {
        await DoSomethingWithTempUserDataAsync(Db, async () =>
        {
            var dto = await Db.Select<User>()
                .Where(u => u.UserId == "test04")
                .ToListAsync<UserDto>(u => new { u.UserId, u.Sign });
            Assert.HasCount(1, dto);
            Assert.AreEqual(SignType.Svip, dto[0].Sign);
        });
    }

    [TestMethod]
    public async Task UpdateTest()
    {
        await DoSomethingWithTempUserDataAsync(Db, async () =>
        {
            var users = await Db.Select<User>().ToListAsync();
            var oldValue = users.Sum(u => u.Age);
            foreach (var item in users)
            {
                item.Age *= 2;
            }
            await Db.Update([.. users]).UpdateColumns(u => u.Age).Set(u => u.Password, "123").ExecuteAsync();
            var newUsers = await Db.Select<User>().ToListAsync();
            var newValue = newUsers.Sum(u => u.Age);
            Assert.IsTrue(newUsers.All(u => u.Password == "123"));
            Assert.AreEqual(oldValue * 2, newValue);
        });
    }

    [TestMethod]
    public async Task DeleteTest()
    {
        await DoSomethingWithTempUserDataAsync(Db, async () =>
        {
            var users = await Db.Select<User>().ToListAsync();
            var cc = users.Count;
            var less10 = users.Where(u => u.Age < 10).ToArray();
            // 批量删除
            var dc = await Db.Delete(less10).ExecuteAsync();
            users = await Db.Select<User>().ToListAsync();
            Assert.HasCount(cc - dc, users);
            var vip = users.Where(u => u.Sign == SignType.Svip).First();

            dc = await Db.Delete(vip).Where(u => u.Age > 20).ExecuteAsync();
            Assert.AreEqual(0, dc);

            var id = -1;
            dc = await Db.Delete(vip).Where(u => u.Id == id).ExecuteAsync();
            Assert.AreEqual(0, dc);

            dc = await Db.Delete(vip).ExecuteAsync();
            Assert.AreEqual(1, dc);

            dc = await Db.Delete<User>().Where(u => u.UserRoles.Any(ur => ur.RoleId == "Admin")).ExecuteAsync();
            Assert.AreEqual(1, dc);
        });
    }
}
