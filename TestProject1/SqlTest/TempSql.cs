using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.SqlTest
{
    [TestClass]
    public class TempSql : TestBase
    {
        [TestMethod]
        public void DeclareTemp()
        {
            var tempU = Db.Select<User>().GroupBy(u => new { u.UserId }).ToSelect(g => new
            {
                g.Group.UserId,
                Total = g.Count()
            }).AsTemp("us");

            var tempR = Db.Select<Role>().WithTempQuery(tempU)
                .Where((r, u) => r.RoleId == u.UserId)
                .Where(w=> w.Tb2.UserId.StartsWith("ad"))
                .AsTemp("temp",w=>new
                {
                    w.Tb1.RoleId,
                    w.Tb2.UserId,
                });

            var sql = Db.Select<Power>().WithTempQuery(tempU, tempR)
                .Where(w => w.Tb2.Total > 10 || w.Tb3.UserId.Contains("admin"))
                .ToSql();

            Console.WriteLine(sql);
        }
    }
}
