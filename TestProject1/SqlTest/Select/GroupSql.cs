using LightORM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.SqlTest.Select;

[TestClass]
public class GroupSql : TestBase
{
    [TestMethod]
    public void GroupSelect()
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
        var result = """
            SELECT `a`.`USER_ID` AS `UserId`, `b`.`ROLE_ID` AS `RoleId`, COUNT(*) AS `Total`, COUNT(CASE WHEN (`a`.`AGE` > 10) THEN 1 ELSE NULL END) AS `Pass`, MAX(CASE WHEN (`a`.`AGE` > 10) THEN `a`.`USER_NAME` ELSE 0 END) AS `NoPass`
            FROM `USER` `a`
            INNER JOIN `USER_ROLE` `b` ON (`a`.`USER_ID` = `b`.`USER_ID`)
            GROUP BY `a`.`USER_ID`, `b`.`ROLE_ID`
            HAVING COUNT(*) > 10 AND MAX(`a`.`AGE`) > 18
            ORDER BY `a`.`USER_ID` ASC
            """;
        Assert.IsTrue(result == sql);
    }

    [TestMethod]
    public void GroupByReturn()
    {
        var sql = Db.Select<User>()
            .InnerJoin<UserRole>((u, ur) => u.UserId == ur.UserId)
            .GroupBy(w => new { w.Tb1.UserId })
            .ToSql();
        Console.WriteLine(sql);
        var result = """
            SELECT `a`.`USER_ID`
            FROM `USER` `a`
            INNER JOIN `USER_ROLE` `b` ON (`a`.`USER_ID` = `b`.`USER_ID`)
            GROUP BY `a`.`USER_ID`
            """;
        Assert.IsTrue(result == sql);
    }

    [TestMethod]
    public void GroupBy_Sub_InnerJoin()
    {
        var sql = Db.Select<User>()
                .GroupBy(a => new { a.UserId })
                .AsTable(g => new
                {
                    g.Group.UserId,
                    Total = g.Count(),
                    Tb = g.Count<int?>(g.Tables.Age > 18 ? 1 : null)
                })
                .InnerJoin<Power>((a, s) => a.UserId == s.PowerId)
                .ToSql((a, s) => new
                {
                    Jyjgbh = a.UserId,
                    a.Total,
                    a.Tb,
                    Jczmc = s.PowerName
                });
        Console.WriteLine(sql);
        var result = """
            SELECT `a`.`UserId`, `a`.`Total`, `a`.`Tb`, `b`.`POWER_NAME` AS `Jczmc`
            FROM (
                SELECT `a`.`USER_ID` AS `UserId`, COUNT(*) AS `Total`, COUNT(CASE WHEN (`a`.`AGE` > 18) THEN 1 ELSE NULL END) AS `Tb`
                FROM `USER` `a`
                GROUP BY `a`.`USER_ID`
            ) `a`
            INNER JOIN `POWERS` `b` ON (`a`.`UserId` = `b`.`POWER_ID`)
            """;
        Assert.IsTrue(result == sql);
    }
}
