using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.SqlTest
{
    [TestClass]
    public class UnionSql : TestBase
    {
        [TestMethod]
        public void Union()
        {
            var sql = Db.Select<User>().Union(Db.Select<User>())
                .Where(u => u.Age > 10)
                .ToSql();
            Console.WriteLine(sql);
        }

        [TestMethod]
        public void UnionParams()
        {
            var sql = Db.Union(Db.Select<User>(), Db.Select<User>())
                .Where(u => u.Age > 10)
                .ToSql();
            Console.WriteLine(sql);
        }
    }
}
