using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.FlatSqlTest;

[TestClass]
public class SelectTest : TestBase
{
    [TestMethod]
    public void SelectFlat()
    {
        var sql = Db.Select<UserFlat>().ToList().ToArray();
        Console.WriteLine(sql);
    }
}
