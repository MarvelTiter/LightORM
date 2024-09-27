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
            //  SELECT *
            //  FROM `USER` `a5`
            //  LEFT JOIN (
            //      SELECT `a1`.`ProductId`, COUNT(*) AS `Total`
            //      FROM `Product` `a1`
            //      GROUP BY `a1`.`ProductId`
            //  ) `temp0` ON ( `a5`.`AGE` = `temp0`.`ProductId` )
            //  WHERE ( `temp0`.`Total` > 10 )
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
            //  SELECT * 
            //  FROM (
            //      SELECT `a5`.`USER_ID` AS `UserId`, COUNT(*) AS `Total` 
            //      FROM `USER` `a5`
            //      WHERE ( `a5`.`AGE` > 10 )
            //      GROUP BY `a5`.`USER_ID`
            //  ) `temp0`
            //  WHERE `temp0`.`UserId` LIKE '%'||@Const_0||'%'
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
