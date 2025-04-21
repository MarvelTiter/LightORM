using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.ExpressionTest;

[TestClass]
public class ConstraintTest : TestBase
{
    [TestMethod]
    public void ConstraintValue()
    {
        Expression<Func<User, bool>> where = u => u.UserName.Contains("Hello");
        var t1 = TableContext.GetTableInfo(typeof(User))!;
        ResolveCtx.AddSelectedTable(t1);
        var result = where.Resolve(SqlResolveOptions.Where, ResolveCtx);
        Console.WriteLine(result.SqlString);
    }
    [TestMethod]
    public void VariableValue()
    {
        var key = "Hello";
        Expression<Func<User, bool>> where = u => u.UserName.Contains(key);
        var t1 = TableContext.GetTableInfo(typeof(User))!;
        ResolveCtx.AddSelectedTable(t1);
        var result = where.Resolve(SqlResolveOptions.Where, ResolveCtx);
        Console.WriteLine(result.SqlString);
    }

    [TestMethod]
    public void ConstraintValues()
    {
        string[] names = ["S1", "S2"];
        Expression<Func<User, bool>> where = u => u.UserName.In("S1", "S2");
        var t1 = TableContext.GetTableInfo(typeof(User))!;
        ResolveCtx.AddSelectedTable(t1);
        var result = where.Resolve(SqlResolveOptions.Where, ResolveCtx);
        Console.WriteLine(result.SqlString);
    }

    [TestMethod]
    public void BooleanValueTest()
    {
        Expression<Func<User, bool>> where = u => u.IsLock == true;
        var t1 = TableContext.GetTableInfo(typeof(User))!;
        ResolveCtx.AddSelectedTable(t1);
        var result = where.Resolve(SqlResolveOptions.Where, ResolveCtx);
        Console.WriteLine(result.SqlString);
    }

    [TestMethod]
    public void BooleanValueTest2()
    {
        var b = "12345".Length > 4;
        Expression<Func<User, bool>> where = u => u.IsLock == b;
        var t1 = TableContext.GetTableInfo(typeof(User))!;
        ResolveCtx.AddSelectedTable(t1);
        var result = where.Resolve(SqlResolveOptions.Where, ResolveCtx);
        Console.WriteLine(result.SqlString);
    }
}
