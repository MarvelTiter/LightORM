using LightORM.Providers.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LightORMTest.ResolverTest;

[TestClass]
public class ValueResolveTest
{
    class Pa(int a)
    {
        public int A => a;
    }
    [TestMethod]
    public void ResolveVariable()
    {
        Test(new(10));
        Test(new(20));
        static void Test(Pa a)
        {
            var p = "ad";
            Expression<Func<User, bool>> where = u => u.Age > a.A && u.UserName.Contains(p);
            var context = new ResolveContext(CustomSqlite.TestInstance);
            var result = where.Resolve(SqlResolveOptions.Where, context);
            Assert.IsNotNull(result.ResolvedValues);
            Assert.HasCount(2, result.ResolvedValues);
            Assert.AreEqual(a.A, result.ResolvedValues[0].Value);
            Assert.AreEqual("ad", result.ResolvedValues[1].Value);
        }
    }

    [TestMethod]
    public void ResolveArray()
    {
        int[] arr = [10, 9, 8, 7, 6, 5, 4, 3, 2, 1];
        int? i = GetIndex();
        var ii = new { index = 5 };

        var context = new ResolveContext(CustomSqlite.TestInstance);

        Expression<Func<User, bool>> where1 = u => u.Age == arr[i.Value];
        var nullableInt = where1.Resolve(SqlResolveOptions.Where, context);
        Assert.AreEqual(7, nullableInt.ResolvedValues![0].Value);

        Expression<Func<User, bool>> where2 = u => u.Age == arr[ii.index];
        var anonymous = where2.Resolve(SqlResolveOptions.Where, context);
        Assert.AreEqual(5, anonymous.ResolvedValues![0].Value);

        static int GetIndex()
        {
            return 3;
        }
    }

    [TestMethod]
    public void ResolveCacheTest()
    {
        int[] arr = [10, 9, 8, 7, 6, 5, 4, 3, 2, 1];
        int? i = GetIndex();
        string s = "ad";
        var ii = new { index = 5 };

        var context = new ResolveContext(CustomSqlite.TestInstance);

        Expression<Func<User, bool>> withVariable = u => u.Age > arr[i.Value] && u.Age < arr[ii.index] && u.UserName.Contains(s);
        var result1 = withVariable.Resolve(SqlResolveOptions.Where, context);
        var result2 = withVariable.Resolve(SqlResolveOptions.Where, context);
        Assert.IsNotNull(result1.ResolvedValues);
        Assert.IsNotNull(result2.ResolvedValues);
        Assert.AreEqual(7, result1.ResolvedValues[0].Value);
        Assert.AreEqual(7, result2.ResolvedValues[0].Value);
        Assert.HasCount(3, result1.ResolvedValues);
        Assert.HasCount(3, result2.ResolvedValues);

        for (int j = 0; j < result1.ResolvedValues.Count; j++)
        {
            Assert.AreEqual(result1.ResolvedValues[j].Value, result2.ResolvedValues[j].Value);
            Assert.AreEqual(result1.ResolvedValues[j].Name, result2.ResolvedValues[j].Name);
        }

        Expression<Func<User, bool>> noVariable = u => u.Age > 7 && u.Age < 18 && u.UserName.Contains("123");
        var result3 = noVariable.Resolve(SqlResolveOptions.Where, context);
        var result4 = noVariable.Resolve(SqlResolveOptions.Where, context);
        Assert.IsNull(result3.ResolvedValues);
        Assert.IsNull(result4.ResolvedValues);
        static int GetIndex()
        {
            return 3;
        }
    }

    [TestMethod]
    public void ResolveCacheArrayContain()
    {
        int[] arr = [10, 9, 8, 7, 6, 5, 4, 3, 2, 1];
        var context = new ResolveContext(CustomSqlite.TestInstance);
        Expression<Func<User, bool>> inArray = u => arr.Contains(u.Age!.Value);
        var result1 = inArray.Resolve(SqlResolveOptions.Where, context);
        var result2 = inArray.Resolve(SqlResolveOptions.Where, context);
        Assert.IsNotNull(result1.ResolvedValues);
        Assert.IsNotNull(result2.ResolvedValues);
        for (int j = 0; j < result1.ResolvedValues.Count; j++)
        {
            Assert.AreEqual(result1.ResolvedValues[j].Value, result2.ResolvedValues[j].Value);
            Assert.AreEqual(result1.ResolvedValues[j].Name, result2.ResolvedValues[j].Name);
        }
    }
}
