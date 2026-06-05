using LightORM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest.ExpressionTest;

[TestClass]
public class ConstantExpressionTest
{
    [TestMethod]
    public void TestConstantNullValue()
    {
        Expression<Func<User, bool>> exp = u => u.Age == null;
        var result = exp.Resolve(SqlResolveOptions.Where, new ResolveContext(LightORM.Providers.Sqlite.CustomSqliteAdapter.TestInstance));
        Console.WriteLine(result.SqlString);
        Assert.AreEqual("(`a`.`AGE` IS NULL)", result.SqlString);
    }

    [TestMethod]
    public void TestVaribleValue()
    {
        var resolveCtx = new ResolveContext(LightORM.Providers.Sqlite.CustomSqliteAdapter.TestInstance);
        var resolveOption = SqlResolveOptions.Where;
        var age = 10;
        Expression<Func<User, bool>> exp = u => u.Age > age;
        var result = exp.Resolve(resolveOption, resolveCtx);
        Console.WriteLine(result.SqlString);
        Assert.AreEqual("(`a`.`AGE` > age_0_0)", result.SqlString);

        var p = ExpressionValueExtract.Default.Extract(exp, resolveOption, resolveCtx);
        Assert.AreEqual("age_0_0", p[0].Name);
    }
}
