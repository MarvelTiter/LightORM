using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest.SqlGenerate;

public partial class SelectSql
{
    [TestMethod]
    public void TestSelectWithGroupBy()
    {
        var sql = Db.Select<User>()
            .InnerJoin<UserRole>(w => w.Tb1.UserId == w.Tb2.UserId)
            .GroupBy(w => new { w.Tb1.UserId, w.Tb2.RoleId })
            .Having(w => w.Count() > 10 && w.Max(w.Tables.Tb1.Age) > 18)
            .OrderBy(w => w.Group.UserId)
            .ToSql(w => new
            {
                w.Group.UserId,
                w.Group.RoleId,
                Total = w.Count(),
                Pass = w.Count<int?>(w.Tables.Tb1.Age > 10 ? 1 : null),
                NoPass = w.Max(w.Tables.Tb1.Age > 10, w.Tables.Tb1.UserName)
            });
        Console.WriteLine(sql);
        AssertSqlResult(nameof(TestSelectWithGroupBy), sql);

    }

    [TestMethod]
    public void TestGroupByWithSubQuery()
    {
        var sql = Db.Select<User>()
            .GroupBy(a => new { a.UserId })
            .AsTable(g => new
            {
                g.Group.UserId,
                Total = g.Count(),
                Tb = g.Count<int?>(g.Tables.Age > 18 ? 1 : null)
            })
            .AsSubQuery()
            .InnerJoin<Permission>((a, s) => a.UserId == s.PermissionId)
            .ToSql((a, s) => new
            {
                Jyjgbh = a.UserId,
                a.Total,
                a.Tb,
                Jczmc = s.PermissionName
            });
        Console.WriteLine(sql);
        AssertSqlResult(nameof(TestGroupByWithSubQuery), sql);
    }

    [TestMethod]
    public void TestGroupBy_Rollup()
    {
        var sql = Db.Select<Sales>()
            .GroupBy(s => new { s.Region, s.Province, s.Product })
            .Rollup()
            .ToSql(g => new
            {
                region = g.Coalesce("合计", g.Group.Region),
                province = g.Coalesce("小计", g.Group.Province),
                product = g.Coalesce("小计", g.Group.Product),
                flag1 = g.Grouping(g.Group.Region),
                flag2 = g.Grouping(g.Group.Province),
                flag3 = g.Grouping(g.Group.Product),
                total = g.Sum(g.Tables.Amount)
            });
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void TestGroupBy_Cube()
    {
        var sql = Db.Select<Sales>()
            .GroupBy(s => new { s.Region, s.Province, s.Product })
            .Cube()
            .ToSql(g => new
            {
                region = g.Coalesce("合计", g.Group.Region),
                province = g.Coalesce("小计", g.Group.Province),
                product = g.Coalesce("小计", g.Group.Product),
                flag1 = g.Grouping(g.Group.Region),
                flag2 = g.Grouping(g.Group.Province),
                flag3 = g.Grouping(g.Group.Product),
                total = g.Sum(g.Tables.Amount)
            });
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void TestGroupBy_GroupingSets()
    {
        var sql = Db.Select<Sales>()
            .GroupBy(s => new { s.Region, s.Province, s.Product })
            //.AddGroupingSet(g => new { g.Region, g.Province, g.Product })
            //.AddGroupingSet(g => new { g.Region, g.Province })
            //.AddGroupingSet(g => new { })
            .GroupingSets(sets => sets
                .Set(g => new { g.Region, g.Province, g.Product })
                .Set(g => new { g.Region, g.Province })
                .Set(g => new { })
            )
            .AsTable(g => new
            {
                region = g.Coalesce("合计", g.Group.Region),
                province = g.Coalesce("小计", g.Group.Province),
                product = g.Coalesce("小计", g.Group.Product),
                flag1 = g.Grouping(g.Group.Region),
                flag2 = g.Grouping(g.Group.Province),
                flag3 = g.Grouping(g.Group.Product),
                total = g.Sum(g.Tables.Amount)
            }).AsSubQuery()
            .Where(r => r.flag1 == 0 || r.flag2 == 0)
            .ToSql(r => new
            {
                r.region,
                r.province,
                r.product,
                r.total
            });
        Console.WriteLine(sql);
    }
}
