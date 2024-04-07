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

        var sql = Context.Select<Power, RolePower, UserRole>(w => new { w.Tb1, w.Tb2.RoleId })
                                      .Distinct()
                                      .InnerJoin<RolePower>(w => w.Tb1.PowerId == w.Tb2.PowerId)
                                      .InnerJoin<UserRole>(w => w.Tb2.RoleId == w.Tb3.RoleId)
                                      .Where(w => w.Tb3.UserId == "admin")
                                      .OrderBy(w => w.Tb1.Sort)
                                      .ToSql();
    }

    [TestMethod]
    public void SelectFunc()
    {
        var sql = Context.Select<Product>(p => new
        {
            Code = SqlFn.Sum(p.ProductId > 100, p.ProductId)
        }).ToSql();
    }

    [TestMethod]
    public void NavigateSelect()
    {
        var sql = Context.Select<User>().Where(u => u.UserRoles.Any(ur => ur.RoleName.Contains("管理"))).ToSql();


        Context.Select<User, UserRole, Role>()
            .InnerJoin<UserRole>(w => w.Tb1.UserId == w.Tb2.UserId)
            .InnerJoin<Role>(w => w.Tb3.RoleId == w.Tb2.RoleId)
            .Where(w => w.Tb3.RoleName.Contains("管理"))
            .ToSql();

    }

}
