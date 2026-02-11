using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest;

public class DbMethodTest : TestBase
{
    [TestMethod]
    public void TestToString()
    {
        var today = DateTime.Now;
        Expression<Func<User, bool>> exp = u => u.LastLogin!.Value.ToString("yyyy-MM-dd") == today.ToString("yyyy-MM-dd");
        var r1 = exp.Resolve(SqlResolveOptions.Where, ResolveCtx);
        var r2 = exp.Resolve(SqlResolveOptions.Where, ResolveCtx);
        Console.WriteLine(r1.SqlString);
    }
}
