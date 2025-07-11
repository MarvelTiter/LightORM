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
        Expression<Func<User, string?>> exp = u => u.LastLogin.Value.ToString("yyyy-MM-dd");
        var sql = exp.Resolve(SqlResolveOptions.Where, ResolveCtx);
        Console.WriteLine(sql.SqlString);
    }
}
