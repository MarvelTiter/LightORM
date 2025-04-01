using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.ResultTest
{
    [TestClass]
    public class SelectResult : TestBase
    {
        class UU
        {
            public string? UserName { get; set; }
        }
        [TestMethod]
        public void SelectList()
        {
            var list = Db.Select<User>()
                .InnerJoin<UserRole>((u, ur) => u.UserId == ur.UserId)
                .ToDynamicList(w => new { w.Tb1.UserName });
            foreach (var item in list)
            {
                Console.WriteLine(item.UserName);
            }
        }

        class PP : Power
        {
            [LightORM.LightColumn(Name = "ROLE_ID")]
            public string? RoleId { get; set; }
        }
        [TestMethod]
        public void SelectExt()
        {
            var list = Db.Select<Power>()
                  .InnerJoin<RolePower>((p, rp) => p.PowerId == rp.PowerId)
                  .ToList<PP>((p, rp) => new()
                  {
                      RoleId = rp.RoleId,
                      PowerId = p.PowerId,
                      PowerName = p.PowerName
                  });
            foreach (var item in list)
            {
                Console.WriteLine($"{item.RoleId}-{item.PowerId}-{item.PowerName}");
            }
        }
    }
}
