using LightORM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1;

[TestClass]
public class TypeSetFlatTest : TestBase
{
    [TestMethod]
    public void Flat()
    {
        Expression<Func<TypeSet<User, Power>, bool>> exp = w => w.Tb1.Age > 10;
        Expression<Func<User, Power, object>> result = (u, p) => new { u };

        var n = FlatTypeSet.Default.Flat(exp);
        ExpressionResolver resolver = new ExpressionResolver(SqlResolveOptions.Select);
        var nn = n.Resolve(SqlResolveOptions.Select);
    }

    [TestMethod]
    public void FlatNew()
    {
        Expression<Func<TypeSet<User, Power>, object>> exp = w => new { w.Tb1.UserName, w.Tb2.Path };
        var n = FlatTypeSet.Default.Flat(exp);
        Console.WriteLine(exp);
        Console.WriteLine(n);
    }

    [TestMethod]
    public void FlatGroup()
    {
        Expression<Func<IExpGroupSelectResult<string, TypeSet<User, UserRole>>, object>> exp = w => new
        {
            w.Group,
            Total = w.Count(),
            Pass = w.Count(w.Tables.Tb1.Age)
        };
        // (user, userrole, w) => new { w.Group, Total = w.Count(), Pass = w.Count(user.Age)  }
        var n = FlatTypeSet.Default.Flat(exp);
        Console.WriteLine(exp);
        Console.WriteLine(n);
    }
}
