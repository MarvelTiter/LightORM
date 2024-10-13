using LightORM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.SqlTest
{
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
        }

        [TestMethod]
        public void GroupByReturn()
        {
            var sql = Db.Select<User>()
                .InnerJoin<UserRole>((u, ur) => u.UserId == ur.UserId)
                .GroupBy(w => new { w.Tb1.UserId })
                .ToSql();
            Console.WriteLine(sql);
        }
    }
}
