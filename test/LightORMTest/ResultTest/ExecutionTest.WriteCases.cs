using System.Text;

namespace LightORMTest.ResultTest;

/// <summary>
/// IExpInsert / IExpUpdate / IExpDelete / IExpSelectGroup 写操作与分组接口方法覆盖用例（基类）
/// 覆盖：InsertColumns / IgnoreColumns / Set / SetIf / InsertByName / InsertByNames /
///      ReturnIdentity / UpdateByName / UpdateByNames / UpdateIgnoreColumns / SetNull /
///      SetNullIf / WithVersion / Delete.WhereIf / FullDelete(truncate) / 分组 Having / Rollup
/// </summary>
public partial class ExecutionTest
{
    [TestMethod]
    public async Task Insert_Columns_Test()
    {
        // InsertColumns 仅插入指定列
        await Db.Insert(new User { UserId = "ins01", UserName = "Ins1", Password = "p", Age = 20, Sign = SignType.Vip, IsLock = false, Version = 1 })
            .InsertColumns(u => new { u.UserId, u.UserName, u.Password, u.Age, u.Sign, u.IsLock, u.Version })
            .ExecuteAsync(TestContext.CancellationToken);
        var u = await Db.Select<User>().Where(x => x.UserId == "ins01").FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(u);
        Assert.AreEqual("Ins1", u.UserName);
        Assert.AreEqual(20, u.Age);
        Assert.AreEqual(SignType.Vip, u.Sign);
    }

    [TestMethod]
    public async Task Insert_IgnoreColumns_Test()
    {
        // IgnoreColumns 忽略指定列（Avator 不入库）
        var user = new User { UserId = "ins02", UserName = "Ins2", Password = "p", Age = 22, Sign = SignType.None, IsLock = false, Version = 1, Avator = Encoding.UTF8.GetBytes("ins02") };
        await Db.Insert(user)
            .IgnoreColumns(u => new { u.Avator })
            .ExecuteAsync(TestContext.CancellationToken);
        var u = await Db.Select<User>().Where(x => x.UserId == "ins02").FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(u);
        Assert.AreEqual(22, u.Age);
    }

    [TestMethod]
    public async Task Insert_Set_SetIf_Test()
    {
        // Set 覆盖字段值，SetIf 按条件决定是否覆盖
        await Db.Insert(new User { UserId = "ins03", UserName = "Ins3", Password = "p", Age = 1, Sign = SignType.None, IsLock = false, Version = 1 })
            .Set(u => u.Age, 33)
            .SetIf(true, u => u.UserName, "SetIfApplied")
            .SetIf(false, u => u.UserName, "SetIfIgnored")
            .ExecuteAsync(TestContext.CancellationToken);
        var u = await Db.Select<User>().Where(x => x.UserId == "ins03").FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(u);
        Assert.AreEqual(33, u.Age);
        Assert.AreEqual("SetIfApplied", u.UserName);
    }

    [TestMethod]
    public async Task Insert_ByName_Test()
    {
        // InsertByName 按属性名指定插入列与值
        await Db.Insert(new User { UserId = "ins04", UserName = "Ins4", Password = "p", Sign = SignType.None, IsLock = false, Version = 1 })
            .InsertByName(nameof(User.UserName), "ByName")
            .InsertByName(nameof(User.Password), "p")
            .InsertByName(nameof(User.Sign), SignType.Vip)
            .InsertByName(nameof(User.IsLock), false)
            .ExecuteAsync(TestContext.CancellationToken);
        var u = await Db.Select<User>().Where(x => x.UserId == "ins04").FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(u);
        Assert.AreEqual("ByName", u.UserName);
        Assert.AreEqual(SignType.Vip, u.Sign);
    }

    [TestMethod]
    public async Task Insert_ByNames_Test()
    {
        // InsertByNames 批量指定列名与值
        await Db.Insert(new User { UserId = "ins05", UserName = "x", Password = "x", Sign = SignType.None, IsLock = false, Version = 1 })
            .InsertByNames([nameof(User.UserName), nameof(User.Password), nameof(User.Sign), nameof(User.IsLock)],
                ["ByNames", "p", SignType.Svip, true])
            .ExecuteAsync(TestContext.CancellationToken);
        var u = await Db.Select<User>().Where(x => x.UserId == "ins05").FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(u);
        Assert.AreEqual("ByNames", u.UserName);
        Assert.AreEqual(SignType.Svip, u.Sign);
    }

    [TestMethod]
    public async Task Insert_ReturnIdentity_Test()
    {
        // ReturnIdentity 自增主键回写
        var id = await Db.Insert(new User { UserId = "ins06", UserName = "Ins6", Password = "p", Sign = SignType.None, IsLock = false, Version = 1 })
            .ReturnIdentity()
            .ExecuteAsync(TestContext.CancellationToken);
        Assert.IsTrue(id >= 0);
        var u = await Db.Select<User>().Where(x => x.UserId == "ins06").FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(u);
    }

    [TestMethod]
    public async Task Update_ByName_Test()
    {
        await Db.Update<User>()
            .UpdateByName(nameof(User.UserName), "ByNameUpdated")
            .Where(u => u.UserId == "test01")
            .ExecuteAsync(TestContext.CancellationToken);
        var u = await Db.Select<User>().Where(x => x.UserId == "test01").FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(u);
        Assert.AreEqual("ByNameUpdated", u.UserName);
    }

    [TestMethod]
    public async Task Update_ByNames_Test()
    {
        await Db.Update<User>()
            .UpdateByNames([nameof(User.UserName), nameof(User.Age)], ["ByNamesUpdated", 99])
            .Where(u => u.UserId == "test02")
            .ExecuteAsync(TestContext.CancellationToken);
        var u = await Db.Select<User>().Where(x => x.UserId == "test02").FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(u);
        Assert.AreEqual("ByNamesUpdated", u.UserName);
        Assert.AreEqual(99, u.Age);
    }

    [TestMethod]
    public async Task Update_IgnoreColumns_Test()
    {
        // 更新实体时忽略 Age 列
        var u = await Db.Select<User>().Where(x => x.UserId == "test03").FirstAsync(TestContext.CancellationToken);
        u!.UserName = "IgnoredCol";
        u.Age = 77;
        await Db.Update(u)
            .IgnoreColumns(x => new { x.Age })
            .ExecuteAsync(TestContext.CancellationToken);
        var updated = await Db.Select<User>().Where(x => x.UserId == "test03").FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(updated);
        Assert.AreEqual("IgnoredCol", updated.UserName);
        Assert.AreEqual(8, updated.Age); // Age 被忽略，保持原值
    }

    [TestMethod]
    public async Task Update_SetNull_SetNullIf_Test()
    {
        // SetNull 将列置 NULL
        await Db.Update<User>()
            .SetNull(u => u.Age)
            .Where(u => u.UserId == "test04")
            .ExecuteAsync(TestContext.CancellationToken);
        var u = await Db.Select<User>().Where(x => x.UserId == "test04").FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(u);
        Assert.IsNull(u.Age);

        // SetNullIf(true) 生效
        await Db.Update<User>()
            .SetNullIf(true, u => u.LastLogin)
            .Where(u => u.UserId == "test04")
            .ExecuteAsync(TestContext.CancellationToken);

        // SetNullIf(false) 不生效，Age 仍为 NULL
        await Db.Update<User>()
            .SetNullIf(false, u => u.Age)
            .Where(u => u.UserId == "test04")
            .ExecuteAsync(TestContext.CancellationToken);
        u = await Db.Select<User>().Where(x => x.UserId == "test04").FirstAsync(TestContext.CancellationToken);
        Assert.IsNull(u!.Age);
    }

    [TestMethod]
    public async Task Update_SetIf_Test()
    {
        await Db.Update<User>()
            .SetIf(true, u => u.UserName, "SetIfTrue")
            .Where(u => u.UserId == "test05")
            .ExecuteAsync(TestContext.CancellationToken);
        await Db.Update<User>()
            .SetIf(false, u => u.UserName, "SetIfFalse")
            .Where(u => u.UserId == "test05")
            .ExecuteAsync(TestContext.CancellationToken);
        var u = await Db.Select<User>().Where(x => x.UserId == "test05").FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(u);
        Assert.AreEqual("SetIfTrue", u.UserName);
    }

    [TestMethod]
    public async Task Update_WithVersion_Conflict_Test()
    {
        // 版本不匹配 -> 0 行
        var rows = await Db.Update<User>()
            .Set(u => u.UserName == "v2")
            .WithVersion(u => u.Version, 999)
            .Where(u => u.UserId == "test01")
            .ExecuteAsync(TestContext.CancellationToken);
        Assert.AreEqual(0, rows);

        // 版本匹配（初始化数据 Version=0）-> 1 行
        rows = await Db.Update<User>()
            .Set(u => u.UserName == "v2")
            .WithVersion(u => u.Version, 0)
            .Where(u => u.UserId == "test01")
            .ExecuteAsync(TestContext.CancellationToken);
        Assert.AreEqual(1, rows);
    }

    [TestMethod]
    public async Task Delete_WhereIf_Test()
    {
        var dc = await Db.Delete<User>()
            .WhereIf(true, u => u.UserId == "test01")
            .WhereIf(false, u => u.UserId == "test02")
            .ExecuteAsync(TestContext.CancellationToken);
        Assert.AreEqual(1, dc);
        Assert.IsNull(await Db.Select<User>().Where(x => x.UserId == "test01").FirstAsync(TestContext.CancellationToken));
        Assert.IsNotNull(await Db.Select<User>().Where(x => x.UserId == "test02").FirstAsync(TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task Delete_FullDelete_Test()
    {
        // FullDelete(truncate: true) 清空表
        await Db.Delete<User>().FullDelete(truncate: true).ExecuteAsync(TestContext.CancellationToken);
        var count = await Db.Select<User>().CountAsync(TestContext.CancellationToken);
        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public async Task Group_Having_Test()
    {
        // 分组 + Having + 分组聚合投影
        var list = await Db.Select<Sales>()
            .GroupBy(s => s.Region)
            .Having(g => g.Count() > 1)
            .ToListAsync(g => new { Region = g.Group, Total = g.Count() }, TestContext.CancellationToken);
        Assert.AreEqual(7, list.Count);
        Assert.IsTrue(list.All(x => x.Total > 1));
    }

    [TestMethod]
    public async Task Group_Rollup_Test()
    {
        // Rollup 分组汇总，额外产生 Region 为 null 的合计行
        var list = await Db.Select<Sales>()
            .GroupBy(s => s.Region)
            .Rollup()
            .ToListAsync(g => new { Region = g.Group, Total = g.Count() }, TestContext.CancellationToken);
        Assert.IsTrue(list.Count >= 7);
        Assert.IsTrue(list.Any(x => x.Region == null));
    }
}
