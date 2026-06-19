using System.Text;

namespace LightORMTest.ResultTest;

public partial class ExecutionTest
{
    [TestInitialize]
    public async Task InitDatas()
    {
        await Db.Delete<User>().FullDelete().ExecuteAsync(TestContext.CancellationToken);
        await Db.Delete<UserRole>().FullDelete().ExecuteAsync(TestContext.CancellationToken);
        await Db.Delete<Role>().FullDelete().ExecuteAsync(TestContext.CancellationToken);
        await Db.Delete<RolePermission>().FullDelete().ExecuteAsync(TestContext.CancellationToken);
        await Db.Delete<Permission>().FullDelete().ExecuteAsync(TestContext.CancellationToken);
        await Db.Insert<User>().InsertEachAsync([
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
        ], TestContext.CancellationToken);
        await Db.Insert<UserRole>().InsertEachAsync([
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
        ], TestContext.CancellationToken);
        await Db.Insert<Role>().InsertEachAsync([
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
        ], TestContext.CancellationToken);
        await Db.Insert<Permission>().InsertEachAsync([
            new Permission()
            {
                PermissionId = "P001",
                PermissionName = "仪表盘",
                ParentId = "",
                PermissionType = PermissionType.Page,
                PermissionLevel = 1,
                Icon = "dashboard",
                Path = "/dashboard",
                Sort = 1
            },
            new Permission()
            {
                PermissionId = "P002",
                PermissionName = "系统管理",
                ParentId = "",
                PermissionType = PermissionType.Page,
                PermissionLevel = 1,
                Icon = "setting",
                Path = "/system",
                Sort = 2
            },
            new Permission()
            {
                PermissionId = "P003",
                PermissionName = "用户管理",
                ParentId = "P002",
                PermissionType = PermissionType.Page,
                PermissionLevel = 2,
                Icon = "user",
                Path = "/system/user",
                Sort = 1
            },
            new Permission()
            {
                PermissionId = "P004",
                PermissionName = "查看用户",
                ParentId = "P003",
                PermissionType = PermissionType.Button,
                PermissionLevel = 3,
                Icon = "",
                Path = "",
                Sort = 1
            },
            new Permission()
            {
                PermissionId = "P005",
                PermissionName = "新增用户",
                ParentId = "P003",
                PermissionType = PermissionType.Button,
                PermissionLevel = 3,
                Icon = "",
                Path = "",
                Sort = 2
            },
            new Permission()
            {
                PermissionId = "P006",
                PermissionName = "编辑用户",
                ParentId = "P003",
                PermissionType = PermissionType.Button,
                PermissionLevel = 3,
                Icon = "",
                Path = "",
                Sort = 3
            },
            new Permission()
            {
                PermissionId = "P007",
                PermissionName = "删除用户",
                ParentId = "P003",
                PermissionType = PermissionType.Button,
                PermissionLevel = 3,
                Icon = "",
                Path = "",
                Sort = 4
            },
            new Permission()
            {
                PermissionId = "P008",
                PermissionName = "角色管理",
                ParentId = "P002",
                PermissionType = PermissionType.Page,
                PermissionLevel = 2,
                Icon = "team",
                Path = "/system/role",
                Sort = 2
            },
            new Permission()
            {
                PermissionId = "P009",
                PermissionName = "查看角色",
                ParentId = "P008",
                PermissionType = PermissionType.Button,
                PermissionLevel = 3,
                Icon = "",
                Path = "",
                Sort = 1
            },
            new Permission()
            {
                PermissionId = "P010",
                PermissionName = "新增角色",
                ParentId = "P008",
                PermissionType = PermissionType.Button,
                PermissionLevel = 3,
                Icon = "",
                Path = "",
                Sort = 2
            },
            new Permission()
            {
                PermissionId = "P011",
                PermissionName = "编辑角色",
                ParentId = "P008",
                PermissionType = PermissionType.Button,
                PermissionLevel = 3,
                Icon = "",
                Path = "",
                Sort = 3
            },
            new Permission()
            {
                PermissionId = "P012",
                PermissionName = "删除角色",
                ParentId = "P008",
                PermissionType = PermissionType.Button,
                PermissionLevel = 3,
                Icon = "",
                Path = "",
                Sort = 4
            }
        ], TestContext.CancellationToken);
        await Db.Insert<RolePermission>().InsertEachAsync([
            new RolePermission()
            {
                RoleId = "Admin",
                PermissionId = "P001"
            },
            new RolePermission()
            {
                RoleId = "Admin",
                PermissionId = "P002"
            },
            new RolePermission()
            {
                RoleId = "Admin",
                PermissionId = "P003"
            },
            new RolePermission()
            {
                RoleId = "Admin",
                PermissionId = "P004"
            },
            new RolePermission()
            {
                RoleId = "Admin",
                PermissionId = "P005"
            },
            new RolePermission()
            {
                RoleId = "Admin",
                PermissionId = "P006"
            },
            new RolePermission()
            {
                RoleId = "Admin",
                PermissionId = "P007"
            },
            new RolePermission()
            {
                RoleId = "Admin",
                PermissionId = "P008"
            },
            new RolePermission()
            {
                RoleId = "Admin",
                PermissionId = "P009"
            },
            new RolePermission()
            {
                RoleId = "Admin",
                PermissionId = "P010"
            },
            new RolePermission()
            {
                RoleId = "Admin",
                PermissionId = "P011"
            },
            new RolePermission()
            {
                RoleId = "Admin",
                PermissionId = "P012"
            },
            new RolePermission()
            {
                RoleId = "SuperAdmin",
                PermissionId = "P001"
            },
            new RolePermission()
            {
                RoleId = "SuperAdmin",
                PermissionId = "P002"
            },
            new RolePermission()
            {
                RoleId = "SuperAdmin",
                PermissionId = "P003"
            },
            new RolePermission()
            {
                RoleId = "SuperAdmin",
                PermissionId = "P004"
            },
            new RolePermission()
            {
                RoleId = "SuperAdmin",
                PermissionId = "P005"
            },
            new RolePermission()
            {
                RoleId = "SuperAdmin",
                PermissionId = "P008"
            },
            new RolePermission()
            {
                RoleId = "SuperAdmin",
                PermissionId = "P009"
            },
            new RolePermission()
            {
                RoleId = "SuperAdmin",
                PermissionId = "P010"
            }
        ], TestContext.CancellationToken);
    }
}
