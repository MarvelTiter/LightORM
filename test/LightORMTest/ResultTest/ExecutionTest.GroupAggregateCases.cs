namespace LightORMTest.ResultTest;

/// <summary>
/// IExpSelectGroup / IExpSelectGrouping 分组高级聚合接口方法覆盖用例（基类）
/// 覆盖：Sum / Sum(条件) / CountDistinct / Average / Round / Count /
///      Coalesce / NullThen / Join(GROUP_CONCAT) / 分组 OrderByDesc + Paging /
///      Rollup / Cube / 分组 First(Async)
/// 数据口径：Sales 共 28 条，7 个区域，各区域金额
///   华东=7500 华南=4800 华北=3700 华中=1550 西南=2200 东北=1350 西北=880（总额 21980）
/// </summary>
public partial class ExecutionTest
{
    [TestMethod]
    public async Task Group_Sum_Test()
    {
        // Sum(列) + Sum(条件, 列)
        var list = await Db.Select<Sales>()
            .GroupBy(s => s.Region)
            .ToListAsync(g => new
            {
                Region = g.Group,
                Total = g.Sum(g.Tables.Amount),
                BigTotal = g.Sum(g.Tables.Amount > 1000, g.Tables.Amount)
            }, TestContext.CancellationToken);

        Assert.HasCount(7, list);
        Assert.AreEqual(7500d, list.First(x => x.Region == "华东").Total, 0.001);
        Assert.AreEqual(4800d, list.First(x => x.Region == "华南").Total, 0.001);
        Assert.AreEqual(3700d, list.First(x => x.Region == "华北").Total, 0.001);
        Assert.AreEqual(880d, list.First(x => x.Region == "西北").Total, 0.001);

        // 条件求和：仅统计 Amount > 1000
        Assert.AreEqual(6700d, list.First(x => x.Region == "华东").BigTotal, 0.001); // 排除 800
        Assert.AreEqual(3500d, list.First(x => x.Region == "华南").BigTotal, 0.001); // 排除 900/400
        Assert.AreEqual(1100d, list.First(x => x.Region == "华北").BigTotal, 0.001); // 排除 600/700/300
        Assert.AreEqual(0d, list.First(x => x.Region == "华中").BigTotal, 0.001);
    }

    [TestMethod]
    public async Task Group_CountDistinct_Test()
    {
        // 每个区域只有一种产品
        var list = await Db.Select<Sales>()
            .GroupBy(s => s.Region)
            .ToListAsync(g => new { Region = g.Group, Products = g.CountDistinct(g.Tables.Product) }, TestContext.CancellationToken);
        Assert.AreEqual(7, list.Count);
        Assert.IsTrue(list.All(x => x.Products == 1));
    }

    [TestMethod]
    public async Task Group_Round_Test()
    {
        // Round(平均值, 保留位数)
        var list = await Db.Select<Sales>()
            .GroupBy(s => s.Region)
            .ToListAsync(g => new
            {
                Region = g.Group,
                Avg = g.Round(g.Average(g.Tables.Amount), 1),
                Count = g.Count()
            }, TestContext.CancellationToken);

        Assert.AreEqual(7, list.Count);
        Assert.AreEqual(1500d, list.First(x => x.Region == "华东").Avg, 0.001); // 7500/5
        Assert.AreEqual(176d, list.First(x => x.Region == "西北").Avg, 0.001);   // 880/5
        Assert.AreEqual(5, list.First(x => x.Region == "华东").Count);
        Assert.AreEqual(3, list.First(x => x.Region == "华南").Count);
    }

    [TestMethod]
    public async Task Group_OrderBy_Paging_Test()
    {
        // 分组 OrderByDesc(聚合) + Paging
        var list = await Db.Select<Sales>()
            .GroupBy(s => s.Region)
            .OrderByDesc(g => g.Sum(g.Tables.Amount))
            .Paging(1, 3)
            .ToListAsync(g => new { Region = g.Group, Total = g.Sum(g.Tables.Amount) }, TestContext.CancellationToken);
        Assert.HasCount(3, list);
        Assert.AreEqual("华东", list[0].Region); // 7500
        Assert.AreEqual("华南", list[1].Region); // 4800
        Assert.AreEqual("华北", list[2].Region); // 3700
    }

    [TestMethod]
    public async Task Group_Rollup_Coalesce_Test()
    {
        // Rollup 产生汇总行，Coalesce 把 null 分组值替换为“合计”
        var list = await Db.Select<Sales>()
            .GroupBy(s => s.Region)
            .Rollup()
            .ToListAsync(g => new { Region = g.Coalesce("合计", g.Group), Total = g.Count() }, TestContext.CancellationToken);
        Assert.HasCount(8, list); // 7 区域 + 1 合计行
        Assert.IsTrue(list.Any(x => x.Region == "合计"));
        Assert.IsTrue(list.Any(x => x.Region == "华东"));
    }

    [TestMethod]
    public async Task Group_Rollup_NullThen_Test()
    {
        // NullThen(列, 回退值)：汇总行 Region 为 null 时回退为“合计”
        var list = await Db.Select<Sales>()
            .GroupBy(s => s.Region)
            .Rollup()
            .ToListAsync(g => new { Region = g.NullThen(g.Group, "合计"), Total = g.Count() }, TestContext.CancellationToken);
        Assert.HasCount(8, list);
        Assert.IsTrue(list.Any(x => x.Region == "合计"));
    }

    [TestMethod]
    public async Task Group_Cube_Test()
    {
        // Cube（单列分组时等价于 Rollup，方言需支持 CUBE 语法）
        var list = await Db.Select<Sales>()
            .GroupBy(s => s.Region)
            .Cube()
            .ToListAsync(g => new { Region = g.Coalesce("合计", g.Group), Total = g.Count() }, TestContext.CancellationToken);
        Assert.IsGreaterThanOrEqualTo(7, list.Count);
        Assert.IsTrue(list.Any(x => x.Region == "合计"));
    }

    [TestMethod]
    public async Task Group_Join_Test()
    {
        // Join(列) 分组内拼接字符串（GROUP_CONCAT / STRING_AGG / LISTAGG）
        var list = await Db.Select<Sales>()
            .GroupBy(s => s.Region)
            .ToListAsync(g => new { Region = g.Group, Provs = g.Join(g.Tables.Province).Separator(",").OrderBy(g.Tables.Province).Value() }, TestContext.CancellationToken);
        Assert.HasCount(7, list);
        var hd = list.First(x => x.Region == "华东");
        Assert.Contains("上海", hd.Provs);
        Assert.Contains("福建", hd.Provs);
    }

    [TestMethod]
    public void Group_First_Test()
    {
        // 分组 First(exp) 同步：按金额升序第一个是 西北(880)
        var first = Db.Select<Sales>()
            .GroupBy(s => s.Region)
            .OrderBy(g => g.Sum(g.Tables.Amount))
            .First(g => new { Region = g.Group, Total = g.Sum(g.Tables.Amount) });
        Assert.IsNotNull(first);
        Assert.AreEqual("西北", first!.Region);
        Assert.AreEqual(880d, first.Total, 0.001);
    }

    [TestMethod]
    public async Task Group_FirstAsync_Test()
    {
        // 分组 FirstAsync(exp)：按金额降序第一个是 华东(7500)
        var first = await Db.Select<Sales>()
            .GroupBy(s => s.Region)
            .OrderByDesc(g => g.Sum(g.Tables.Amount))
            .FirstAsync(g => new { Region = g.Group, Total = g.Sum(g.Tables.Amount) }, TestContext.CancellationToken);
        Assert.IsNotNull(first);
        Assert.AreEqual("华东", first!.Region);
        Assert.AreEqual(7500d, first.Total, 0.001);
    }
}
