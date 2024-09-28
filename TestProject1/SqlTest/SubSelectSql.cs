using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.SqlTest
{
    [TestClass]
    public class SubSelectSql : TestBase
    {
        [TestMethod]
        public void JoinSelect()
        {
            var sql = Db.Select<User>()
                .LeftJoin(Db.Select<Product>().GroupBy(p => new { p.ProductId }).ToSelect(g => new
                {
                    g.Group.ProductId,
                    Total = g.Count()
                }), (u, j) => u.Age == j.ProductId)
                .Where(w => w.Tb2.Total > 10)
                .ToSql();
            Console.WriteLine(sql);
        }

        [TestMethod]
        public void SubQuery()
        {
            var sql = Db.Select<User>().Where(u => u.Age > 10).GroupBy(u => new
            {
                u.UserId
            }).AsTempQuery(u => new
            {
                u.Group.UserId,
                Total = u.Count()
            })
            .Where(t => t.UserId.Contains("admin"))
            .ToSql();
            Console.WriteLine(sql);
        }
    }
}
