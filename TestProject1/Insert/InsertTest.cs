using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.Insert;

[TestClass]
public class InsertTest : TestBase
{
    [TestMethod]
    public void Insert()
    {
        var p = new Product();
        var sql = Context.Insert(p).ToSql();
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void InsertColumn()
    {
        var p = new Product();
        p.ProductCode = "123";
        var sql = Context.Insert(p).SetColumns(p => new { p.ProductCode, p.ProductName }).ToSql();
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void InsertIgnore()
    {
        var p = new Product();
        var sql = Context.Insert(p).IgnoreColumns(p => new { p.CreateTime }).ToSql();
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void InsertGeneric()
    {
        var p = new Power();
        var s = Context.Select<Power>().ToSql();
        var sql = I(p);

        string I<T>(T val) where T : IPower
        {
            return Context.Insert(val).ToSql();
        }
    }

    [TestMethod]
    public void InsertBatch()
    {
        var effect = Context.Insert<Product>(DataHelper.GetProductList()).Execute();
        Assert.IsTrue(effect == 100);
    }
}
