using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest.ResultTest;

public class ExecutionTest : TestBase
{
    private static async Task DoSomethingWithTempUserDataAsync(IExpressionContext db, Func<Task> action)
    {
        db.Delete<User>().FullDelete().Execute();
        db.Delete<UserRole>().FullDelete().Execute();
        db.Delete<Role>().FullDelete().Execute();
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
    }
    [TestMethod]
    public async Task Select_Result_Test()
    {
        await DoSomethingWithTempUserDataAsync(Db, async () =>
        {
            // tolist
            var list1 = await Db.Select<User>().OrderBy(u => u.Age).ToListAsync();
            Assert.HasCount(4, list1);
            Assert.AreEqual(8, list1[0].Age);
            Assert.AreEqual(9, list1[1].Age);
            Assert.AreEqual(11, list1[2].Age);
            Assert.AreEqual(12, list1[3].Age);

            // tolist with count
            int age = 10;
            var list2 = await Db.Select<User>()
                .Where(u => u.Age < age)
                .Count(out var total)
                .OrderByDesc(u => u.Age)
                .ToListAsync();
            Assert.HasCount(2, list2);
            Assert.AreEqual(9, list2[0].Age);

            // to async enumerable list
            var list3 = Db.Select<User>().ToEnumerableAsync();
            await foreach (var item in list3)
            {
                Console.WriteLine($"{item.UserId} - {item.UserName}");
            }

            #region include

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

            #endregion

            // select string column
            var list4 = await Db.Select<User>().ToListAsync(u => u.UserName);
            Assert.HasCount(4, list4);
            Assert.AreEqual("Test1", list4[0]);
            Assert.AreEqual("Test2", list4[1]);

            // select int column
            var list5 = await Db.Select<User>().ToListAsync(u => u.Age);
            Assert.HasCount(4, list5);
            Assert.AreEqual(9, list5[1]);
            Assert.AreEqual(8, list5[2]);

            // select anonymous type
            var list6 = await Db.Select<User>().ToListAsync(u => new { u.UserId, u.UserName, u.Age });
            Assert.HasCount(4, list6);
            Assert.AreEqual("test02", list6[1].UserId);
            Assert.AreEqual("Test2", list6[1].UserName);
            Assert.AreEqual(9, list6[1].Age);

            // select group result
            var list7 = await Db.Select<User>()
                .GroupBy(u => u.IsLock)
                .ToListAsync(g => new
                {
                    IsLock = g.Group,
                    Total = g.Count(),
                    AvgAge = g.Average(g.Tables.Age),
                    MaxAge = g.Max(g.Tables.Age),
                    MinAge = g.Min(g.Tables.Age),
                });
            Assert.HasCount(2, list7);
            Assert.IsTrue(list7.Any(r => r.IsLock == true && r.Total == 2 && r.AvgAge == 10.5 && r.MaxAge == 12 && r.MinAge == 9));
            Assert.IsTrue(list7.Any(r => r.IsLock == false && r.Total == 2 && r.AvgAge == 9.5 && r.MaxAge == 11 && r.MinAge == 8));

            // select projection
            var dto = await Db.Select<User>()
                .Where(u => u.UserId == "test04")
                .ToListAsync<UserDto>(u => new() { UserId = u.UserId, Sign = u.Sign });
            Assert.HasCount(1, dto);
            Assert.AreEqual(SignType.Svip, dto[0].Sign);

            // multi arg
            int arg = 10;
            var list8 = await Db.Select<User>().Where(u => u.Age < arg && u.Version < arg).ToListAsync();
            Assert.HasCount(2, list8);
        });
    }


    class UserDto
    {
        public string? UserId { get; set; }
        public SignType Sign { get; set; }
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
            // 批量更新Age，并且更新password为指定值
            await Db.Update([.. users]).UpdateColumns(u => u.Age).Set(u => u.Password, "123").ExecuteAsync();
            var newUsers = await Db.Select<User>().ToListAsync();
            var newValue = newUsers.Sum(u => u.Age);
            Assert.IsTrue(newUsers.All(u => u.Password == "123"));
            Assert.AreEqual(oldValue * 2, newValue);

            // 通过实体更新指定字段的方式
            var first = newUsers.First();
            await Db.Update(first).Set(u => u.Age, 19).Set(u => u.UserName == "Hello!").ExecuteAsync();
            var newFirst = await Db.Select<User>().Where(u => u.Id == first.Id).FirstAsync();
            Assert.IsNotNull(newFirst);
            Assert.AreEqual("Hello!", newFirst.UserName);
            Assert.AreEqual(19, newFirst.Age);
            Assert.AreEqual(first.Sign, newFirst.Sign);

            // 版本列测试
            Assert.AreEqual(2, newFirst.Version);
            await Db.Update<User>().Set(u => u.Password == "321").Where(u => u.Id == newFirst.Id).ExecuteAsync();
            newFirst = await Db.Select<User>().Where(u => u.Id == newFirst.Id).FirstAsync();
            Assert.AreEqual(2, newFirst!.Version);
            Assert.AreEqual("321", newFirst.Password);

            await Db.Update<User>().Set(u => u.Password == "3212").Where(u => u.Id == newFirst.Id).WithVersion(newFirst.Version).ExecuteAsync();
            newFirst = await Db.Select<User>().Where(u => u.Id == newFirst.Id).FirstAsync();
            Assert.AreEqual(3, newFirst!.Version);
            Assert.AreEqual("3212", newFirst.Password);

            await Db.Update<User>().Set(u => u.Password == "32123").Where(u => u.Id == newFirst.Id).WithVersion(u => u.Version, newFirst.Version).ExecuteAsync();
            newFirst = await Db.Select<User>().Where(u => u.Id == newFirst.Id).FirstAsync();
            Assert.AreEqual(4, newFirst!.Version);
            Assert.AreEqual("32123", newFirst.Password);
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

    [TestMethod]
    public async Task MultiResultTest()
    {
        await DoSomethingWithTempUserDataAsync(Db, async () =>
        {
            using var multi = Db.QueryMultiple(Db.Select<User>(), Db.Select<Role>(), Db.Select<UserRole>());
            var u = await multi.ReadListAsync<User>();
            var r = await multi.ReadListAsync<Role>();
            var ur = await multi.ReadListAsync<UserRole>();
            Assert.HasCount(4, u);
            Assert.HasCount(2, r);
            Assert.HasCount(4, ur);
        });
    }
}
