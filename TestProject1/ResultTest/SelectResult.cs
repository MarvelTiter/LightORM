using System;
using System.Collections.Generic;
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
            var list = Db.Select<User>().ToList<UU>(u => new { u.UserName }).ToList();
            foreach (var item in list)
            {
                Console.WriteLine(item.UserName);
            }

        }
    }
}
