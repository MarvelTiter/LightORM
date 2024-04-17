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
        //var sql1 = Context.Select<Role>().Where(u => u.Users.Any(ur => ur.UserName.Contains("管理"))).ToSql();
        //Console.WriteLine(sql1);

        //var roles = Context.Select<Role>().ToList();

        var sql2 = Context.Select<Power>().Where(p => p.Roles.Any(r => r.RoleId == "Admin")).ToList();
        //Console.WriteLine(sql2);

        //var sql3 = Context.Select<UserRole>().Where(ur => ur.User.UserName.Contains("管理")).ToSql();
        //Console.WriteLine(sql3);

        //var sql4 = Context.Select<Power>().Where(p => p.Children.Any(child => child.PowerName == "123")).ToSql();
        //Console.WriteLine(sql4);


    }

    [TestMethod]
    public void SelectInclude()
    {
        var sql = Context.Select<Power>()
            .Include(p => p.Roles)
            .ThenInclude(r => r.Users.Where(u => u.UserId == "admin"))
            //.Include(p => p.Roles.Where(r => r.Users.Any(u => u.UserName.Contains("admin"))))
            .ToList();
    }
    [TestMethod]
    public void SelectInclude2()
    {
        var sql = Context.Select<User>()
            .Include(p => p.UserRoles)
            .ThenInclude(r => r.Powers)
            //.Include(p => p.Roles.Where(r => r.Users.Any(u => u.UserName.Contains("admin"))))
            .ToList();
    }

    [TestMethod]
    public void SelectDynamic()
    {
        var sql = Context.Select("POWERS").Where("1=1").ToList(() => new
        {
            POWER_ID = ""
        });
    }
}
