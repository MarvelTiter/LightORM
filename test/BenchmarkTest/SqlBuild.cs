using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkTest.Models;
using LightORM;
using LightORM.Providers.Sqlite.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BenchmarkTest;

[MemoryDiagnoser]
public class SqlBuild
{
    class Jobs
    {
        public string? Plate { get; set; }
        public string? StnId { get; set; }
    }
    [GlobalSetup]
    public void Setup()
    {
        ExpSqlFactory.Configuration(config =>
        {
            config.UseSqlite("Data Source=:memory:;Version=3;New=True;");
            config.SetEnableExpressionCache(false);
        });
    }
    [Benchmark]
    public string SelectCTE()
    {
        var Db = ExpSqlFactory.GetContext();
        // 从Jobs表中，选择Plate的第一个字符作为Fzjg字段，选择Fzjg和StnId，作为temp表，并命名为info
        // With info as (...)
        var info = Db.Select<Jobs>().AsTemp("info", j => new
        {
            Fzjg = j.Plate!.Substring(1, 2),
            j.StnId
        });
        // 从info表中，按StnId和Fzjg分组并且按Count(*)排序后，选择StnId，Fzjg，Count(*)，RowNumer，作为temp表，并命名为stn_fzjg，表数据为每个StnId中，按Fzjg数据量进行排序并标记为Index
        var stnFzjg = Db.FromTemp(info)
            .GroupBy(a => new { a.StnId, a.Fzjg })
            .OrderByDesc(a => new { a.Group.StnId, i = a.Count() })
            .AsTemp("stn_fzjg", g => new
            {
                g.Group.StnId,
                g.Group.Fzjg,
                Count = g.Count(),
                Index = WinFn.RowNumber().PartitionBy(g.Tables.StnId).OrderByDesc(g.Count()).Value()
            });
        // 从info表中，按Fzjg分组并且按Count(*)排序后，选择StnId，Fzjg，Count(*)，RowNumer，作为temp表，并命名为all_fzjg，表数据为所有Fzjg中，按每个Fzjg的数据量进行排序并标记为Index
        var allFzjg = Db.FromTemp(info).GroupBy(a => new { a.Fzjg })
            .OrderByDesc(a => a.Count())
            .AsTemp("all_fzjg", g => new
            {
                StnId = "合计",
                Fzjg = g.Group.Fzjg,
                Count = g.Count(),
                Index = WinFn.RowNumber().OrderByDesc(g.Count()).Value()
            });
        // 从info表中，按StnId进行Group By Rollup ，选择StnId和分组数据量，作为temp表，并命名为all_station
        var allStation = Db.FromTemp(info).GroupBy(t => new { t.StnId })
            .Rollup()
            .AsTemp("all_station", g => new
            {
                StnId = SqlFn.NullThen(g.Group.StnId, "合计"),
                Total = SqlFn.Count()
            });
        /*
         * 1. 从stn_fzjg中，筛选出所有前3的Fzjg数量，然后按StnId分组，选择StnId，组内第一Fzjg作为FirstFzjg，组内第一的Count作为FirstCount
         * 2. 从all_fzjg中，筛选出所有前3的Fzjg数量，选择'合计'作为StnId，第一Fzjg作为FirstFzjg，第一的Count作为FirstCount
         * 3. 将1和2的结果Union ALl
         * 4. 转为子查询，inner join all_station
         * 5. select结果列
         */
        var sql = Db.FromTemp(stnFzjg).Where(t => t.Index < 4)
            .GroupBy(t => new { t.StnId })
            .AsTable(g => new
            {
                StnId = g.Group.StnId!,
                FirstFzjg = g.Join(g.Tables.Index == 1 ? g.Tables.Fzjg.ToString() : "").Separator("").OrderBy(g.Tables.StnId).Value(),
                FirstCount = g.Join(g.Tables.Index == 1 ? g.Tables.Count.ToString() : "").Separator("").OrderBy(g.Tables.StnId).Value()
            }).UnionAll(Db.FromTemp(allFzjg).Where(t => t.Index < 4).AsTable(g => new
            {
                StnId = "合计",
                FirstFzjg = SqlFn.Join(g.Index == 1 ? g.Fzjg.ToString() : "").Separator("").OrderBy(g.StnId).Value(),
                FirstCount = SqlFn.Join(g.Index == 1 ? g.Count.ToString() : "").Separator("").OrderBy(g.StnId).Value()
            })).AsSubQuery()
            .InnerJoin(allStation, (t, a) => t.StnId == a.StnId)
            .ToSql((t, a) => new
            {
                Jczmc = SqlFn.NullThen(t.StnId, "TT"),
                a.Total,
                t
            });
        return sql;
    }

    [Benchmark]
    public string SelectCTEOptimized()
    {
        var Db = ExpSqlFactory.GetContext();
        // 从Jobs表中，选择Plate的第一个字符作为Fzjg字段，选择Fzjg和StnId，作为temp表，并命名为info
        // With info as (...)
        var info = Db.Select<Jobs>().AsTemp("info", j => new
        {
            Fzjg = j.Plate!.Substring(1, 2),
            j.StnId
        });
        // 从info表中，按StnId和Fzjg分组并且按Count(*)排序后，选择StnId，Fzjg，Count(*)，RowNumer，作为temp表，并命名为stn_fzjg，表数据为每个StnId中，按Fzjg数据量进行排序并标记为Index
        var stnFzjg = Db.FromTemp(info)
            .GroupBy(a => new { a.StnId, a.Fzjg })
            .OrderByDesc(a => new { a.Group.StnId, i = a.Count() })
            .AsTemp("stn_fzjg", g => new
            {
                g.Group.StnId,
                g.Group.Fzjg,
                Count = g.Count(),
                Index = WinFn.RowNumber().PartitionBy(g.Tables.StnId).OrderByDesc(g.Count()).Value()
            });
        // 从info表中，按Fzjg分组并且按Count(*)排序后，选择StnId，Fzjg，Count(*)，RowNumer，作为temp表，并命名为all_fzjg，表数据为所有Fzjg中，按每个Fzjg的数据量进行排序并标记为Index
        var allFzjg = Db.FromTemp(info).GroupBy(a => a.Fzjg)
            .OrderByDesc(a => a.Count())
            .AsTemp("all_fzjg", g => new
            {
                StnId = "合计",
                Fzjg = g.Group,
                Count = g.Count(),
                Index = WinFn.RowNumber().OrderByDesc(g.Count()).Value()
            });
        // 从info表中，按StnId进行Group By Rollup ，选择StnId和分组数据量，作为temp表，并命名为all_station
        var allStation = Db.FromTemp(info).GroupBy(t => t.StnId)
            .Rollup()
            .AsTemp("all_station", g => new
            {
                StnId = SqlFn.NullThen(g.Group, "合计"),
                Total = SqlFn.Count()
            });
        /*
         * 1. 从stn_fzjg中，筛选出所有前3的Fzjg数量，然后按StnId分组，选择StnId，组内第一Fzjg作为FirstFzjg，组内第一的Count作为FirstCount
         * 2. 从all_fzjg中，筛选出所有前3的Fzjg数量，选择'合计'作为StnId，第一Fzjg作为FirstFzjg，第一的Count作为FirstCount
         * 3. 将1和2的结果Union ALl
         * 4. 转为子查询，inner join all_station
         * 5. select结果列
         */
        var sql = Db.FromTemp(stnFzjg).Where(t => t.Index < 4)
            .GroupBy(t => t.StnId)
            .AsTable(g => new
            {
                StnId = g.Group!,
                FirstFzjg = g.Join(g.Tables.Index == 1 ? g.Tables.Fzjg.ToString() : "").Separator("").OrderBy(g.Tables.StnId).Value(),
                FirstCount = g.Join(g.Tables.Index == 1 ? g.Tables.Count.ToString() : "").Separator("").OrderBy(g.Tables.StnId).Value()
            }).UnionAll(Db.FromTemp(allFzjg).Where(t => t.Index < 4).AsTable(g => new
            {
                StnId = "合计",
                FirstFzjg = SqlFn.Join(g.Index == 1 ? g.Fzjg.ToString() : "").Separator("").OrderBy(g.StnId).Value(),
                FirstCount = SqlFn.Join(g.Index == 1 ? g.Count.ToString() : "").Separator("").OrderBy(g.StnId).Value()
            })).AsSubQuery()
            .InnerJoin(allStation, (t, a) => t.StnId == a.StnId)
            .ToSql((t, a) => new
            {
                Jczmc = SqlFn.NullThen(t.StnId, "TT"),
                a.Total,
                t
            });
        return sql;
    }

    [Benchmark]
    public string SelectWithArrayAccess()
    {
        var Db = ExpSqlFactory.GetContext();
        int[] arr = [10, 9, 11, 2];
        int? i = 2;
        var sql = Db.Select<User>()
            .Where(u => u.Age == arr[i.Value])
            .ToSql();
        return sql;
    }

    [Benchmark]
    public string SelectWithContainArray()
    {
        int?[] arr = [10, 11];
        var Db = ExpSqlFactory.GetContext();
        var select = Db.Select<User>()
            .Where(u => arr.Contains(u.Age));
        return select.ToSql();
    }
}
