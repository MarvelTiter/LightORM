using LightORM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.ExpressionTest;

[TestClass]
public class SelectExpressionTest : TestBase
{
    //TestTableContext tableContext = new TestTableContext();
    [TestMethod]
    public void SelectAll()
    {
        Expression<Func<Product, object>> select = p => p;
        var table = TestTableContext.TestProject1_Models_Product;
        var result = select.Resolve(SqlResolveOptions.Select, ResolveCtx);
        Console.WriteLine(result.SqlString);
    }

    [TestMethod]
    public void SelectColumn()
    {
        Expression<Func<Product, object>> select = p => new { p.ProductId, p.ProductName };
        var table = TestTableContext.TestProject1_Models_Product;
        var result = select.Resolve(SqlResolveOptions.Select, ResolveCtx);
        Console.WriteLine(result.SqlString);
    }

    [TestMethod]
    public void SelectFromTypeSet()
    {
        Expression<Func<TypeSet<Product, User>, object>> select = p => new { p.Tb1.ProductId, p.Tb2.UserId };
        var t1 = TestTableContext.TestProject1_Models_Product;
        var t2 = TestTableContext.TestProject1_Models_User;
        var result = FlatTypeSet.Default.Flat(select).Resolve(SqlResolveOptions.Select, ResolveCtx);
        Console.WriteLine(result.SqlString);
    }
}
