using LightORM.Providers.Sqlite;
using System;
using System.Collections.Generic;
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
            var context = new ResolveContext(CustomSqlite.Instance);
            var result = where.Resolve(SqlResolveOptions.Where, context);
            Assert.IsNotNull(result.DbParameters);
            Assert.HasCount(2, result.DbParameters);
            Assert.AreEqual(a.A, result.DbParameters[0].Value);
            Assert.AreEqual("ad", result.DbParameters[1].Value);
        }
    }

    [TestMethod]
    public void ResolveArray()
    {
        int[] arr = [10, 9, 8, 7, 6, 5, 4, 3, 2, 1];
        int? i = GetIndex();
        var ii = new { index = 5 };

        var context = new ResolveContext(CustomSqlite.Instance);

        Expression<Func<User, bool>> where1 = u => u.Age == arr[i.Value];
        var nullableInt = where1.Resolve(SqlResolveOptions.Where, context);
        Assert.AreEqual(7, nullableInt.DbParameters![0].Value);

        Expression<Func<User, bool>> where2 = u => u.Age == arr[ii.index];
        var anonymous = where2.Resolve(SqlResolveOptions.Where, context);
        Assert.AreEqual(5, anonymous.DbParameters![0].Value);

        static int GetIndex()
        {
            return 3;
        }
    }
}
