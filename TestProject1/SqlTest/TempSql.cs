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
            //  WITH USER AS (
            //      SELECT `a5`.`USER_ID` AS `UserId`, COUNT(*) AS `Total`
            //      FROM `USER` `a5`
            //      GROUP BY `a5`.`USER_ID`
            //  )
            //  SELECT *
            //  FROM `ROLE` `a2`, `USER` `temp0`
            //  WHERE ( `a2`.`ROLE_ID` = `temp0`.`UserId` )
            var tempU = Db.Select<User>().GroupBy(u => new { u.UserId }).ToSelect(g => new
            {
                g.Group.UserId,
                Total = g.Count()
            });
            var sql = Db.Select<Role>().WithTempQuery(tempU)
                .Where((r, u) => r.RoleId == u.UserId)
                .ToSql();
            Console.WriteLine(sql);
        }
    }
}
