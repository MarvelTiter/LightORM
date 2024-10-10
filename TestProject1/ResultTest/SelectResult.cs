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
    }
}
