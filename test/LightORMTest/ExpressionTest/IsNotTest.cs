using LightORM.Interfaces.ExpSql;
using LightORM.Providers.Sqlite;
using LightORM.Utils.Vistors;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace LightORMTest.ExpressionTest;

[TestClass]
public class IsNotTest
{
    
    [TestMethod]
    public void TestMultiBinaryWithNot()
    {
        //string[] items = ["A", "B", "C"];
        //Expression<Func<User, object>> keySelector = u => u.UserName;
        //Expression<Func<IExpSelectGrouping<string, User>, object>> exp = u => !items.Contains(u.Group) && (u.Tables.Password!.StartsWith("H1") || u.Tables.Password.StartsWith("H2"));

        //var flat = FlatGrouping.Default.Flat(exp, keySelector);

        //var result = flat.Resolve(SqlResolveOptions.Select, new(CustomSqliteAdapter.TestInstance));
        //Console.WriteLine(result.SqlString);

       

    }
}
