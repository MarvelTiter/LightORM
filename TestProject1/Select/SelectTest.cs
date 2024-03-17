using LightORM.Providers.Select;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.Select;

[TestClass]
public class SelectTest : TestBase
{
    [TestMethod]
    public void Select()
    {
        var sql = Context.Select<Product>()
            .InnerJoin<ProductV2>((p1, p2) => p1.CategoryId == p2.ProductId)
            .Where(p => p.ProductName.Contains("123")).ToSql();
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void Max()
    {
        var sql = Context.Select<Product>()
            .Where(p => p.ProductName.Contains("123"))
            .Count();
        Console.WriteLine(sql);
    }
}
