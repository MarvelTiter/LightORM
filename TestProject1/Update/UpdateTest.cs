using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.Update;

[TestClass]
public class UpdateTest : TestBase
{
    [TestMethod]
    public void UpdateSet()
    {
        var p = new Product();
        var sql = Context.Update<Product>().Set(p => p.ProductCode, "100").Where(p => p.ProductId == 100).ToSql();
    }
}
