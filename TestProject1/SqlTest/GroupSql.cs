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
                .GroupBy(w => new { w.Tb1.UserId, w.Tb1.UserName })
                .Having(w => w.Count() > 10 && w.Max(w.Tables.Tb1.Age) > 18)
                .OrderBy(w => w.Group.UserId)
                .ToSql(w => new
                {
                    w.Group.UserId,
                    w.Group.UserName,
                    Total = w.Count(),
                    Pass = w.Count(w.Tables.Tb1.Age),
                    NoPass = w.Max(w.Tables.Tb1.Age > 10, w.Tables.Tb1.UserName)
                });
            Console.WriteLine(sql);
        }
    }
}
