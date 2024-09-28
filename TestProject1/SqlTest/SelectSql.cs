using LightORM.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.SqlTest
{
    [TestClass]
    public class SelectSql : TestBase
    {
        [TestMethod]
        public void SelectSingle()
        {
            var sql = Db.Select<Product>()
                .Where(p => p.ModifyTime > DateTime.Now)
                .ToSql(p => new { p.ProductId, p.ProductName });
            Console.WriteLine(sql);
        }

        [TestMethod]
        public void SelectMulti()
        {
            var sql = Db.Select<User>()
                .InnerJoin<UserRole>(w => w.Tb1.UserId == w.Tb2.UserId)
                .InnerJoin<Role>(w => w.Tb2.RoleId == w.Tb3.RoleId)
                .Where(u => u.UserId == "admin")
                .ToSql(w => w.Tb1);
            Console.WriteLine(sql);
        }

        [TestMethod]
        public void SelectExtension()
        {
            var sql = Db.Select<Power, RolePower, Role>()
                .Distinct()
                .Where(w => w.Tb1.PowerId == w.Tb2.PowerId && w.Tb2.RoleId == w.Tb3.RoleId)
                .ToSql(w => new { w.Tb1 });
            Console.WriteLine(sql);
        }

        [TestMethod]
        public void SelectInclude()
        {
            var sql = Db.Select<User>()
                .Where(u => u.UserRoles.When(r => r.RoleId.StartsWith("ad")))
                .ToSql();
            Console.WriteLine(sql);
        }

        [TestMethod]
        public void SelectIncludeWhere()
        {
            var select = Db.Select<User>()
                .Where(u => u.UserRoles.When(r => r.RoleId.Contains("admin")))
                .ToSql();

            Console.WriteLine(select);
        }
    }
}
