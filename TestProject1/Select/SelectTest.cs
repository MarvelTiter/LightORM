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

    [TestMethod]
    public void SelectSet()
    {
        //Context.Select<User>().ToSql();

        //var sql = Context.Select<Power, RolePower, UserRole>(w => new { w.Tb1.PowerId, w.Tb1.PowerName, w.Tb1.ParentId, w.Tb1.PowerType, w.Tb1.PowerLevel, w.Tb1.Icon, w.Tb1.Path, w.Tb1.Sort })
        //                              .Distinct()
        //                              .InnerJoin<RolePower>(w => w.Tb1.PowerId == w.Tb2.PowerId)
        //                              .InnerJoin<UserRole>(w => w.Tb2.RoleId == w.Tb3.RoleId)
        //                              .Where(w => w.Tb3.UserId == "admin")
        //                              .OrderBy(w => w.Tb1.Sort)
        //                              .ToSql();
        //Context.Select<Power>().ToSql();
        var sql2 = Select<Power>();
    }


    private int[] Select<T>() where T : IPower
    {
        return Context.Select<T>().OrderBy(p => p.Sort, false).ToList().Select(p => p.Sort).ToArray();
    }
}
