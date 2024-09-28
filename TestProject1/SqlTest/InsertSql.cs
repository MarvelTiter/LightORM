using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.SqlTest
{
    [TestClass]
    public class InsertSql : TestBase
    {
        [TestMethod]
        public void InsertEntity()
        {
            var p = new Product();
            var sql = Db.Insert(p).ToSql();
            Console.WriteLine(sql);
        }
    }
}
