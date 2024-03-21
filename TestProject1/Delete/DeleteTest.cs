using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.Delete;

[TestClass]
public class DeleteTest : TestBase
{
    [TestMethod]
    public void DeleteEntity()
    {
        var p = new Product() { ProductId = 100 };
        var sql = Context.Delete(p).ToSql();
        Console.WriteLine(sql);
    }
}
