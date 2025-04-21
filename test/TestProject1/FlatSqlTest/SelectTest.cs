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
        var u = Db.Select<UserFlat>()
            .Where(u => u.UserId == "admin")
            .First();
        Assert.IsTrue(u?.PriInfo.Age == 6);
    }

    [TestMethod]
    public void Select_Flat_Where()
    {
        var u = Db.Select<UserFlat>()
            .Where(u => u.PriInfo.Age == 6)
            .First();
        Assert.IsTrue(u?.UserId == "admin");

    }
}
