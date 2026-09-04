using LightORM.Extension;
using System.Data;

namespace LightORMTest.ResultTest;

/// <summary>
/// IExpSelect0 / IExpSelect 查询链接口方法覆盖用例（基类，各方言测试项目继承执行）
/// 覆盖：WhereIf / Where&lt;TTable&gt; / Paging / Skip / Take / Count(out) / Distinct /
///      ToDataTable / 聚合(同步+异步) / Union / UnionAll / AsSubQuery / AsTable /
///      AsTemp + WithTempQuery / FromTemp / FromQuery / 原生SQL / select-insert SQL 生成
/// </summary>
public partial class ExecutionTest
{
    [TestMethod]
    public async Task Select_WhereIf_Test()
    {
        // 条件为 true 时生效
        var list = await Db.Select<User>()
            .WhereIf(true, u => u.Age > 10)
            .ToListAsync(TestContext.CancellationToken);
        Assert.HasCount(4, list);

        // 条件为 false 时不生效
        list = await Db.Select<User>()
            .WhereIf(false, u => u.Age > 100)
            .ToListAsync(TestContext.CancellationToken);
        Assert.HasCount(6, list);
    }

    [TestMethod]
    public async Task Select_WhereGenericTable_Test()
    {
        // join 后通过 Where<TTable> 对关联表过滤
        var list = await Db.Select<Role>()
            .InnerJoin<UserRole>((r, ur) => r.RoleId == ur.RoleId)
            .Where<UserRole>(ur => ur.UserId == "test01")
            .ToListAsync((r, ur) => r, TestContext.CancellationToken);
        Assert.HasCount(1, list);
        Assert.AreEqual("Admin", list[0].RoleId);
    }

    [TestMethod]
    public async Task Select_Paging_Test()
    {
        // Paging(pageIndex, pageSize)，pageIndex 从 1 开始
        var page = await Db.Select<User>()
            .OrderBy(u => u.Age)
            .Paging(2, 2)
            .ToListAsync(TestContext.CancellationToken);
        Assert.HasCount(2, page);
        Assert.AreEqual(11, page[0].Age);
        Assert.AreEqual(12, page[1].Age);

        // Skip / Take
        var skipTake = await Db.Select<User>()
            .OrderBy(u => u.Age)
            .Skip(2).Take(3)
            .ToListAsync(TestContext.CancellationToken);
        Assert.HasCount(3, skipTake);
        Assert.AreEqual(11, skipTake[0].Age);
        Assert.AreEqual(17, skipTake[2].Age);

        // Count(out total) 分页总数
        var paged = await Db.Select<User>()
            .Count(out var total)
            .Paging(1, 2)
            .ToListAsync(TestContext.CancellationToken);
        Assert.HasCount(2, paged);
        Assert.AreEqual(6, total);
    }

    [TestMethod]
    public async Task Select_Distinct_Test()
    {
        // Distinct + 单列投影 -> SELECT DISTINCT REGION
        var regions = Db.Select<Sales>().Distinct().ToList(s => s.Region).ToList();
        Assert.HasCount(7, regions);
        Assert.Contains("华东", regions);
        Assert.Contains("西北", regions);
    }

    [TestMethod]
    public async Task Select_ToDataTable_Test()
    {
        var dt = Db.Select<User>().Where(u => u.Age > 10).ToDataTable();
        Assert.HasCount(4, dt.Rows);
        Assert.IsTrue(dt.Columns.Cast<DataColumn>().Any(c => string.Equals(c.ColumnName, "USER_ID", StringComparison.OrdinalIgnoreCase)));

        var dt2 = await Db.Select<User>().ToDataTableAsync(TestContext.CancellationToken);
        Assert.HasCount(6, dt2.Rows);
    }

    [TestMethod]
    public void Select_Aggregate_Sync_Test()
    {
        // 同步聚合：Max / Min / Sum / Avg / Count / Any
        Assert.AreEqual(21, Db.Select<User>().Max(u => u.Age));
        Assert.AreEqual(8, Db.Select<User>().Min(u => u.Age));
        Assert.AreEqual(78d, Db.Select<User>().Sum(u => u.Age), 0.001);
        Assert.AreEqual(13d, Db.Select<User>().Avg(u => u.Age), 0.001);
        Assert.AreEqual(6, Db.Select<User>().Count());
        Assert.AreEqual(4, Db.Select<User>().Where(u => u.Age > 10).Count());
        Assert.IsTrue(Db.Select<User>().Any());
        Assert.IsFalse(Db.Select<User>().Where(u => u.Age > 100).Any());
    }

    [TestMethod]
    public async Task Select_Aggregate_Async_Test()
    {
        // 异步聚合：MaxAsync / MinAsync / SumAsync / AvgAsync / CountAsync / AnyAsync
        Assert.AreEqual(21, await Db.Select<User>().MaxAsync(u => u.Age, TestContext.CancellationToken));
        Assert.AreEqual(8, await Db.Select<User>().MinAsync(u => u.Age, TestContext.CancellationToken));
        Assert.AreEqual(78d, await Db.Select<User>().SumAsync(u => u.Age, TestContext.CancellationToken), 0.001);
        Assert.AreEqual(13d, await Db.Select<User>().AvgAsync(u => u.Age, TestContext.CancellationToken), 0.001);
        Assert.AreEqual(6, await Db.Select<User>().CountAsync(TestContext.CancellationToken));
        Assert.AreEqual(4, await Db.Select<User>().Where(u => u.Age > 10).CountAsync(TestContext.CancellationToken));
        Assert.IsTrue(await Db.Select<User>().AnyAsync(TestContext.CancellationToken));
    }

    [TestMethod]
    public void Select_Union_Test()
    {
        // Union 去重
        var list = Db.Select<User>().Where(u => u.Age > 10)
            .Union(Db.Select<User>().Where(u => u.Age > 15))
            .ToList();
        Assert.HasCount(4, list);

        // UnionAll 不去重
        var list2 = Db.Select<User>().Where(u => u.UserId == "test01")
            .UnionAll(Db.Select<User>().Where(u => u.UserId == "test01"))
            .ToList();
        Assert.HasCount(2, list2);

        // IExpressionContext.Union 多查询嵌套
        var list3 = Db.Union(
            Db.Select<User>().Where(u => u.UserId == "test01"),
            Db.Select<User>().Where(u => u.UserId == "test02"),
            Db.Select<User>().Where(u => u.UserId == "test03")).ToList();
        Assert.HasCount(3, list3);
    }

    [TestMethod]
    public async Task Select_AsSubQuery_Test()
    {
        // AsSubQuery 包装为子查询后可继续排序/过滤
        var list = await Db.Select<User>()
            .Where(u => u.Age > 10)
            .AsSubQuery()
            .OrderBy(u => u.Age)
            .ToListAsync(TestContext.CancellationToken);
        Assert.HasCount(4, list);
        Assert.AreEqual(11, list[0].Age);
    }

    [TestMethod]
    public async Task Select_AsTable_Test()
    {
        // AsTable 投影为匿名表 + AsSubQuery 后继续 Where
        var list = await Db.Select<User>()
            .Where(u => u.Age > 10)
            .AsTable(u => new { u.UserId, Age = u.Age ?? 0 })
            .AsSubQuery()
            .Where(d => d.Age < 20)
            .ToListAsync(TestContext.CancellationToken);
        Assert.HasCount(2, list);
        Assert.IsTrue(list.All(d => d.Age < 20));
    }

    [TestMethod]
    public async Task Select_AsTemp_WithTempQuery_Test()
    {
        // CTE：年龄>10 的用户集合，与 USER 表按 UserId 关联
        var temp = Db.Select<User>().Where(u => u.Age > 10).AsTemp("old_users", u => new { u.UserId });
        var list = await Db.Select<User>()
            .WithTempQuery(temp)
            .Where((u, t) => u.UserId == t.UserId)
            .ToListAsync((u, t) => u, TestContext.CancellationToken);
        Assert.HasCount(4, list);
    }

    [TestMethod]
    public async Task Select_FromTemp_Test()
    {
        var temp = Db.Select<Role>().Where(r => r.RoleId == "Admin").AsTemp("admins", r => new { r.RoleId });
        var roles = await Db.FromTemp(temp).ToListAsync(r => r.RoleId, TestContext.CancellationToken);
        Assert.HasCount(1, roles);
        Assert.AreEqual("Admin", roles[0]);
    }

    [TestMethod]
    public async Task Select_FromQuery_Test()
    {
        var list = await Db.FromQuery(Db.Select<User>().Where(u => u.Age > 10))
            .ToListAsync(TestContext.CancellationToken);
        Assert.HasCount(4, list);
    }

    [TestMethod]
    public async Task Select_RawSql_Test()
    {
        var prefix = CurrentDefaultProvider.DatabaseAdapter.Prefix;

        // WithParameters + Where(string) 共享参数
        var list = await Db.Select<User>()
            .WithParameters(new { Age = 10 })
            .Where($"age > {prefix}Age")
            .ToListAsync(TestContext.CancellationToken);
        Assert.HasCount(4, list);

        // Where<TParameter>(string, parameters)
        var list2 = await Db.Select<User>()
            .Where($"age > {prefix}Age", new { Age = 15 })
            .ToListAsync(TestContext.CancellationToken);
        Assert.HasCount(2, list2);

        // WhereIf(string) 条件控制
        var list3 = await Db.Select<User>()
            .WithParameters(new { Age = 10 })
            .WhereIf(true, $"age > {prefix}Age")
            .WhereIf(false, $"age > 999")
            .ToListAsync(TestContext.CancellationToken);
        Assert.HasCount(4, list3);

        // 原生 OrderBy
        var list4 = await Db.Select<User>()
            .OrderBy("age desc")
            .ToListAsync(TestContext.CancellationToken);
        Assert.HasCount(6, list4);
        Assert.AreEqual(21, list4[0].Age);
    }

    [TestMethod]
    public void Select_Insert_Sql_Test()
    {
        // select-insert 链路的 SQL 生成覆盖（不执行，避免方言约束差异）
        var sql = Db.Select<User>()
            .Where(u => u.UserId == "test01")
            .SelectColumns(u => new { u.UserId, u.UserName })
            .Insert<UserFlat>(u => new { u.UserId, u.UserName })
            .ToSql();
        StringAssert.StartsWith(sql, "INSERT INTO");
    }
}
