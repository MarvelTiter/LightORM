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
        [TestMethod]
        public void SelectList()
        {
            var list = Db.Select<User>().ToList().ToList();
        }
    }
}
