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
            //var sql = Db.Select<User>()
            //    .InnerJoin<UserRole>(w => w.Tb1.UserId == w.Tb2.UserId)
            //    .GroupBy(w => w.Tb1.UserId)
            //    .ToSql(w => new
            //    {
            //        w.Group,
            //        Total = w.Count(),
            //        Pass = w.Count(w.Tables.Tb1.Age)
            //    });

            Expression<Func<IExpGroupSelectResult<string, TypeSet<User, UserRole>>, object>> exp = w => new
            {
                w.Group,
                Total = w.Count(),
                Pass = w.Count(w.Tables.Tb1.Age)
            };
            // (string, user, userrole) => new { string,  }
            var flated = FlatTypeSet.Default.Flat(exp);

            //Console.WriteLine(sql);
        }
    }
}
