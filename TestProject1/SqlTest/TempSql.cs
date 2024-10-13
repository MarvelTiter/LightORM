using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TestProject1.SqlTest
{
    [TestClass]
    public class TempSql : TestBase
    {
        [TestMethod]
        public void DeclareTemp()
        {
            var tempU = Db.Select<User>().GroupBy(u => new { u.UserId }).AsTemp("us", g => new
            {
                g.Group.UserId,
                Total = g.Count()
            });

            var tempR = Db.Select<Role>().WithTempQuery(tempU)
                .Where((r, u) => r.RoleId == u.UserId)
                .Where(w => w.Tb2.UserId.StartsWith("ad"))
                .AsTemp("temp", w => new
                {
                    w.Tb1.RoleId,
                    w.Tb2.UserId,
                });

            var tempT = Db.FromTemp(tempR).Where(a => a.RoleId.StartsWith("h")).AsTemp("hh");

            var sql = Db.Select<Power>().WithTempQuery(tempT)
                .Where(w => w.Tb2.UserId.Contains("admin"))
                .ToSql();

            Console.WriteLine(sql);
        }

        public void SelectFromTemp()
        {

        }

        [TestMethod]
        public void GroupByThenAsTemp()
        {
            // TODO
            //WITH temp AS(
            //    SELECT sub.jylsh, AVG(sub.datediff) avgdiff FROM(
            //      SELECT iim.jylsh, (iim.KSSJ - LAG(IIM.KSSJ) OVER(PARTITION BY iim.JYLSH ORDER BY iim.KSSJ)) * 24  DateDiff  FROM
            //      INSPECT_ITEMDATA_M1 iim
            //      WHERE iim.KSSJ > :{ nameof(DetectReq.StartTime)}
            //      AND iim.KSSJ < :{ nameof(DetectReq.EndTime)}
            //      ) sub
            //    WHERE sub.datediff IS NOT NULL
            //    GROUP BY sub.jylsh
            //    HAVING COUNT(*) > 2 AND AVG(sub.datediff) < 1
            //    )
            var dt = DateTime.Now;
            var temp = Db.Select<User>().Where(u => u.LastLogin > dt).AsSubQuery(u => new
            {
                Id = u.UserId,
                DateDiff = (u.LastLogin - WinFn.Lag(u.LastLogin).PartitionBy(u.UserId).OrderBy(u.LastLogin).Value()) * 24
            }, "sub")
                .Where(a => a.DateDiff != null)
                .GroupBy(a => new { a.Id })
                .Having(g => g.Count() > 2 && g.Avg(g.Tables.DateDiff) < 1).AsTemp("temp", g =>
                new
                {
                    g.Group.Id,
                    AvgDiff = g.Avg(g.Tables.DateDiff)
                });
            var sql = temp.SqlBuilder.ToSqlString();
            Console.WriteLine(sql);
        }
    }
}
