using System.Text;

namespace LightORMTest.ResultTest;

public partial class ExecutionTest
{
    private static async Task DoSomethingWithTempUserDataAsync(IExpressionContext db, Func<Task> action)
    {
        await db.Delete<User>().FullDelete().ExecuteAsync();
        await db.Delete<UserRole>().FullDelete().ExecuteAsync();
        await db.Delete<Role>().FullDelete().ExecuteAsync();
        await db.Delete<RolePermission>().FullDelete().ExecuteAsync();
        await db.Delete<Permission>().FullDelete().ExecuteAsync();
        await db.Insert<User>().InsertEachAsync([
            new User()
            {
                UserId = "test01",
                UserName = "Test1",
                Age = 11,
                IsLock = false,
                Password = "helloworld",
                Avator = Encoding.UTF8.GetBytes("test01")
            },
            new User()
            {
                UserId = "test02",
                UserName = "Test2",
                Age = 9,
                IsLock = true,
                Password = "helloworld",
                Avator = Encoding.UTF8.GetBytes("test02")
            },
            new User()
            {
                UserId = "test03",
                UserName = "Test3",
                Age = 8,
                IsLock = false,
                Password = "helloworld",
                Avator = Encoding.UTF8.GetBytes("test03")
            },
            new User()
            {
                UserId = "test04",
                UserName = "Test4",
                Age = 12,
                IsLock = true,
                Password = "helloworld",
                Sign = SignType.Svip
            }
        ]);
        await db.Insert<UserRole>().InsertEachAsync([
            new UserRole()
            {
                UserId = "test01",
                RoleId = "Admin"
            },
            new UserRole()
            {
                UserId = "test02",
                RoleId = "SuperAdmin"
            },
            new UserRole()
            {
                UserId = "test03",
                RoleId = "Admin"
            },
            new UserRole()
            {
                UserId = "test03",
                RoleId = "SuperAdmin"
            }
        ]);
        await db.Insert<Role>().InsertEachAsync([
            new Role()
            {
                RoleId = "Admin",
                RoleName = "管理员"
            },
            new Role()
            {
                RoleId = "SuperAdmin",
                RoleName = "超级管理员"
            }
        ]);
        await action();
    }
}