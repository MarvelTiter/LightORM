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

}
