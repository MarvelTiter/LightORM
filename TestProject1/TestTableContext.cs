using LightORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    [LightORMTableContext]
    internal partial class TestTableContext
    {
        public void H(Type type)
        {
            if (type == typeof(int) || type.IsAssignableFrom(typeof(int)))
            {

            }
        }
    }

    [TestClass]
    public class TableContextTestor
    {
        [TestMethod]
        public void TestTableContext()
        {
            var context = new TestTableContext();
            var t1 = context.GetTableInfo(typeof(User))!;
            t1.CustomName = "Test";
            var t2 = context.GetTableInfo(typeof(User))!;
            Assert.IsFalse(t1.CustomName == t2.CustomName);
        }
    }
}
