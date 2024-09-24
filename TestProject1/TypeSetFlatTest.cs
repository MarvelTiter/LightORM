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
        var nn = n.Resolve(SqlResolveOptions.Select, TestTableContext.TestProject1_Models_User, TestTableContext.TestProject1_Models_Power);
    }
}
