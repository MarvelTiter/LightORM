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
                .ToSql(w => new { w.Tb1 });
            Console.WriteLine(sql);
        }

        [TestMethod]
        public void SelectInclude()
        {
            var select = Db.Select<User>()
                .Include(u => u.UserRoles)
                .ThenInclude(r => r.Powers);
            var includeBuilder1 = IncludeContextExtensions.BuildIncludeSqlBuilder(DbBaseType.Sqlite, new User(), select.SqlBuilder.IncludeContext.Includes[0]);
            var includeBuilder2 = IncludeContextExtensions.BuildIncludeSqlBuilder(DbBaseType.Sqlite, new Role(), select.SqlBuilder.IncludeContext.ThenInclude!.Includes[0]);
            var includeSql1 = includeBuilder1.ToSqlString();
            var includeSql2 = includeBuilder2.ToSqlString();

            Console.WriteLine(includeSql1);
            Console.WriteLine(includeSql2);
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
