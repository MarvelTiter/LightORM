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
        await Db.Delete<Sales>().FullDelete().ExecuteAsync(TestContext.CancellationToken);
        await Db.Insert([
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
        ]).ExecuteAsync(TestContext.CancellationToken);
        await Db.Insert([
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
        ]).ExecuteAsync(TestContext.CancellationToken);
        await Db.Insert([
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
        ]).ExecuteAsync(TestContext.CancellationToken);
        await Db.Insert([
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
        ]).ExecuteAsync(TestContext.CancellationToken);
        await Db.Insert([
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
        ]).ExecuteAsync(TestContext.CancellationToken);
        await Db.Insert([
                new Sales { Region = "华东", Province = "上海", Product = "笔记本电脑", Amount = 1500, Version = 1 },
                new Sales { Region = "华东", Province = "江苏", Product = "笔记本电脑", Amount = 2200, Version = 1 },
                new Sales { Region = "华东", Province = "浙江", Product = "笔记本电脑", Amount = 1800, Version = 1 },
                new Sales { Region = "华东", Province = "安徽", Product = "笔记本电脑", Amount = 800, Version = 1 },
                new Sales { Region = "华东", Province = "福建", Product = "笔记本电脑", Amount = 1200, Version = 1 },
                new Sales { Region = "华南", Province = "广东", Product = "智能手机", Amount = 3500, Version = 1 },
                new Sales { Region = "华南", Province = "广西", Product = "智能手机", Amount = 900, Version = 1 },
                new Sales { Region = "华南", Province = "海南", Product = "智能手机", Amount = 400, Version = 1 },
                new Sales { Region = "华北", Province = "北京", Product = "台式电脑", Amount = 1100, Version = 1 },
                new Sales { Region = "华北", Province = "天津", Product = "台式电脑", Amount = 600, Version = 1 },
                new Sales { Region = "华北", Province = "河北", Product = "台式电脑", Amount = 1000, Version = 1 },
                new Sales { Region = "华北", Province = "山西", Product = "台式电脑", Amount = 700, Version = 1 },
                new Sales { Region = "华北", Province = "内蒙古", Product = "台式电脑", Amount = 300, Version = 1 },
                new Sales { Region = "华中", Province = "河南", Product = "服务器", Amount = 500, Version = 1 },
                new Sales { Region = "华中", Province = "湖北", Product = "服务器", Amount = 600, Version = 1 },
                new Sales { Region = "华中", Province = "湖南", Product = "服务器", Amount = 450, Version = 1 },
                new Sales { Region = "西南", Province = "四川", Product = "平板电脑", Amount = 900, Version = 1 },
                new Sales { Region = "西南", Province = "贵州", Product = "平板电脑", Amount = 350, Version = 1 },
                new Sales { Region = "西南", Province = "云南", Product = "平板电脑", Amount = 400, Version = 1 },
                new Sales { Region = "西南", Province = "重庆", Product = "平板电脑", Amount = 550, Version = 1 },
                new Sales { Region = "东北", Province = "辽宁", Product = "显示器", Amount = 650, Version = 1 },
                new Sales { Region = "东北", Province = "吉林", Product = "显示器", Amount = 300, Version = 1 },
                new Sales { Region = "东北", Province = "黑龙江", Product = "显示器", Amount = 400, Version = 1 },
                new Sales { Region = "西北", Province = "陕西", Product = "键盘鼠标", Amount = 350, Version = 1 },
                new Sales { Region = "西北", Province = "甘肃", Product = "键盘鼠标", Amount = 200, Version = 1 },
                new Sales { Region = "西北", Province = "青海", Product = "键盘鼠标", Amount = 100, Version = 1 },
                new Sales { Region = "西北", Province = "宁夏", Product = "键盘鼠标", Amount = 80, Version = 1 },
                new Sales { Region = "西北", Province = "新疆", Product = "键盘鼠标", Amount = 150, Version = 1 }
            ]).ExecuteAsync(TestContext.CancellationToken);
    }
}
