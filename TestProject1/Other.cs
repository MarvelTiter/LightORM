using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    [TestClass]
    public class Other
    {
        [TestMethod]
        public void M()
        {
            StringBuilder sb = new();
            sb.Append("Hello World!");
            sb.Insert(5, " Insert");
        }
    }
}
