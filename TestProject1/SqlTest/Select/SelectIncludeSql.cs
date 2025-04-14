using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.SqlTest.Select;

[TestClass]
public class SelectIncludeSql : TestBase
{
    [TestMethod]
    public void SelectInclude()
    {
        var sql = Db.Select<User>()
            .Where(u => u.UserRoles.WhereIf(r => r.RoleId.StartsWith("ad")))
            .Include(u => u.UserRoles)
            .ToList();
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void SelectIncludeWhere()
    {
        var select = Db.Select<User>()
            .Where(u => u.UserRoles.WhereIf(r => r.RoleId.Contains("admin")))
            .ToSql();

        Console.WriteLine(select);
    }
}
