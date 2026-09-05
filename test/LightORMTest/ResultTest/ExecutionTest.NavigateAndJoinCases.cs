namespace LightORMTest.ResultTest;

/// <summary>
/// IExpSelect 导航加载(Include)与多来源 Join / WithTempQuery 接口方法覆盖用例（基类）
/// 覆盖：Include(引用导航) / Include(集合导航) / LeftJoin / RightJoin /
///      InnerJoin(string 表名) / InnerJoin(子查询) / InnerJoin(临时表) / WithTempQuery(多临时表)
/// </summary>
public partial class ExecutionTest
{
    [TestMethod]
    public async Task Select_Include_Profile_Test()
    {
        // 引用导航 Profile（一对一，User.UserId <-> UserProfile.UserId）
        var result = await Db.Select<User>()
            .Include(u => u.Profile)
            .Where(u => u.UserId == "test01")
            .FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Profile);
        Assert.AreEqual("13800138001", result.Profile.Phone);
        Assert.AreEqual("北京市朝阳区建国路1号", result.Profile.Address);
    }

    [TestMethod]
    public async Task Select_Include_MultiRole_Test()
    {
        // 集合导航 UserRoles（多对多），test03 拥有 Admin + SuperAdmin 两个角色
        var result = await Db.Select<User>()
            .Include(u => u.UserRoles)
            .Where(u => u.UserId == "test03")
            .FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.UserRoles);
        Assert.AreEqual(2, result.UserRoles.Count());
        Assert.IsTrue(result.UserRoles.Any(r => r.RoleId == "Admin"));
        Assert.IsTrue(result.UserRoles.Any(r => r.RoleId == "SuperAdmin"));
    }

    [TestMethod]
    public async Task Select_LeftJoin_Test()
    {
        // LeftJoin 泛型实体
        var list = await Db.Select<User>()
            .LeftJoin<UserRole>((u, ur) => u.UserId == ur.UserId)
            .Where(u => u.UserId == "test01")
            .ToListAsync((u, ur) => new { u.UserId, RoleId = ur.RoleId }, TestContext.CancellationToken);
        Assert.HasCount(1, list);
        Assert.AreEqual("Admin", list[0].RoleId);
    }

    [TestMethod]
    public async Task Select_RightJoin_Test()
    {
        // RightJoin（注意：个别方言 SQLite 需 3.39+ 才支持）
        var list = await Db.Select<User>()
            .RightJoin<UserRole>((u, ur) => u.UserId == ur.UserId)
            .ToListAsync((u, ur) => new { UserId = u.UserId, RoleId = ur.RoleId }, TestContext.CancellationToken);
        Assert.HasCount(7, list); // 7 条 UserRole 全部保留
    }

    [TestMethod]
    public async Task Select_Join_TableName_Test()
    {
        // InnerJoin(string tableName, ...) 按原始表名关联
        var list = await Db.Select<User>()
            .InnerJoin<UserRole>("USER_ROLE", (u, ur) => u.UserId == ur.UserId)
            .Where(u => u.UserId == "test01")
            .ToListAsync((u, ur) => new { u.UserId, RoleId = ur.RoleId }, TestContext.CancellationToken);
        Assert.HasCount(1, list);
        Assert.AreEqual("Admin", list[0].RoleId);
    }

    [TestMethod]
    public async Task Select_Join_SubQuery_Test()
    {
        // InnerJoin(IExpSelect<TJoin> 子查询, ...)
        var list = await Db.Select<User>()
            .InnerJoin<UserRole>(Db.Select<UserRole>().Where(ur => ur.UserId == "test01"), (u, ur) => u.UserId == ur.UserId)
            .ToListAsync((u, ur) => new { u.UserId, ur.RoleId }, TestContext.CancellationToken);
        Assert.HasCount(1, list);
        Assert.AreEqual("Admin", list[0].RoleId);
    }

    [TestMethod]
    public async Task Select_Join_Temp_Test()
    {
        // InnerJoin(IExpTemp<TJoin> 临时表, ...)：只关联 Normal 角色的用户
        var temp = Db.Select<UserRole>().Where(ur => ur.RoleId == "Normal").AsTemp("normal_ur", ur => new { ur.UserId, ur.RoleId });
        var list = await Db.Select<User>()
            .InnerJoin(temp, (u, ur) => u.UserId == ur.UserId)
            .ToListAsync((u, ur) => new { u.UserId, ur.RoleId }, TestContext.CancellationToken);
        Assert.HasCount(3, list); // test04 / test05 / test06
        Assert.IsTrue(list.All(x => x.RoleId == "Normal"));
    }

    [TestMethod]
    public async Task Select_WithTempQuery_Two_Test()
    {
        // WithTempQuery 多临时表：年龄>10 且拥有 Admin 角色的用户（= test01）
        var temp1 = Db.Select<User>().Where(u => u.Age > 10).AsTemp("old_users", u => new { u.UserId });
        var temp2 = Db.Select<Role>().Where(r => r.RoleId == "Admin").AsTemp("admins", r => new { r.RoleId });
        var list = await Db.Select<User>()
            .WithTempQuery(temp1, temp2)
            .Where((u, t1, t2) => u.UserId == t1.UserId && t2.RoleId == "Admin")
            .ToListAsync((u, t1, t2) => u, TestContext.CancellationToken);
        Assert.HasCount(4, list);
        Assert.AreEqual("test01", list[0].UserId);
    }
}
