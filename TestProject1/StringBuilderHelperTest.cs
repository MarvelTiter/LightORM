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

        [TestMethod]
        public void Trim()
        {
            var sb = new StringBuilder("  Hello123212Owdssa   ");
            var r1 = sb.Trim();
            Assert.IsTrue(r1 == "Hello123212Owdssa");
            sb.AppendLine("");
            Assert.IsTrue(sb.EndsWith("\r\n"));
            r1 = sb.Trim();
            Assert.IsTrue(r1 == "Hello123212Owdssa");
            r1 = sb.Trim(' ', '\r', '\n');
            Assert.IsTrue(r1 == "Hello123212Owdssa");
        }
    }
}
