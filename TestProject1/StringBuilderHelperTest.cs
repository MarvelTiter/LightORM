using LightORM.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    [TestClass]
    public class StringBuilderHelperTest
    {
        [TestMethod]
        public void EndsWith()
        {
            var sb = new StringBuilder("Hello123212Owdssa");
            Assert.IsTrue(sb.EndsWith("wdssa"));
            Assert.IsFalse(sb.EndsWith("wdss"));
        }
    }
}
