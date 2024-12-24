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
                .LeftJoin(Db.Select<Product>().GroupBy(p => new { p.ProductId }).AsTable(g => new
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
            }).AsTable(u => new
            {
                u.Group.UserId,
                Total = u.Count()
            }).AsSubQuery()
            .Where(t => t.UserId.Contains("admin"))
            .ToSql();
            Console.WriteLine(sql);
        }

        [TestMethod]
        public void JoinMultiJoin()
        {
            var temp = Db.Select<User>()
                .InnerJoin<UserRole>((u, ur) => u.UserId == ur.UserId)
                .InnerJoin<Role>(w => w.Tb2.RoleId == w.Tb3.RoleId)
                .AsTable(w => new
                {
                    w.Tb1.UserId,
                    w.Tb1.UserName,
                    w.Tb3.RoleName,
                });
            var sql = Db.Select<User>()
                .LeftJoin<Power>(w => w.Tb1.UserId == w.Tb2.PowerId)
                .LeftJoin(temp, (u, _, t) => u.UserId == t.UserId)
                .ToSql();
            Console.WriteLine(sql);
        }

        [TestMethod]
        public void SubContextQuery()
        {
            var result = Db.Select<User>().Where(u => u.Age > Db.Select<UserRole>().Where(ur => ur.RoleId.Contains("admin")).Result(ur => ur.RoleId).Result<int>()).ToSql();
            Console.WriteLine(result);
        }

        [TestMethod]
        public void SubQueryExits()
        {
            var result = Db.Select<User>().Where(u => Db.Select<UserRole>().Where(ur => ur.RoleId.Contains("admin")).Exits()).ToSql();
            result.AsSpan();
            Console.WriteLine(result);
        }

        [TestMethod]
        public void SubSelect()
        {
            var result = Db.Select<User>().ToSql(u => new
            {
                u.UserName,
                Age = Db.Select<User>().Count(u => u.UserId)
            });
            Console.WriteLine(result);
        }
    }
}
