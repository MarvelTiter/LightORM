using LightORM.Providers.MySql;
using LightORM.Providers.Oracle;
using LightORM.Providers.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.SqlFunctionTest;

[TestClass]
public class SqlFunTest : TestBase
{
    [TestMethod]
    public void RowNumber()
    {
        // ROW_NUMBER() OVER( PARTITION BY `u`.`USER_NAME` ORDER BY `u`.`AGE` ) AS `RowNum`
        Expression<Func<User, object>> exp = (u) => new
        {
            RowNum = WinFn.RowNumber().PartitionBy(u.UserName).OrderBy(u.Age).Value()
        };
        var table = TestTableContext.TestProject1_Models_User;
        table.Alias = "u";
        var ctx = new ResolveContext(CustomSqlite.Instance, table);
        var result = exp.Resolve(SqlResolveOptions.Select, ctx);
        Console.WriteLine(result.SqlString);
    }

    [TestMethod]
    public void Lag()
    {
        // ( `u`.`LAST_LOGIN` - LAG(`u`.`LAST_LOGIN`) OVER( PARTITION BY `u`.`USER_NAME` ORDER BY `u`.`AGE` ) ) AS `DateDiff`
        Expression<Func<User, object>> exp = (u) => new
        {
            DateDiff = u.LastLogin - WinFn.Lag(u.LastLogin).PartitionBy(u.UserName).OrderBy(u.Age).Value()
        };
        var table = TestTableContext.TestProject1_Models_User;
        table.Alias = "u";
        var ctx = new ResolveContext(CustomSqlite.Instance, table);
        var result = exp.Resolve(SqlResolveOptions.Select, ctx);
        Console.WriteLine(result.SqlString);
    }

    [TestMethod]
    public void CaseWhen()
    {
        Expression<Func<User, object>> exp = u => new
        {
            Total = SqlFn.Count(SqlFn.Case<int?>().When(u.Age > 10).Then(u.Age).Else(0).End())
        };
        var table = TestTableContext.TestProject1_Models_User;
        table.Alias = "u";
        var ctx = new ResolveContext(CustomSqlite.Instance, table);
        var result = exp.Resolve(SqlResolveOptions.Select, ctx);
        Console.WriteLine(result.SqlString);
    }

    [TestMethod]
    public void Join()
    {
        Expression<Func<User, object>> exp = u => new
        {
            Result = SqlFn.Join(u.UserName).Separator("|").Distinct().OrderBy(u.UserId).Value()
        };
        var table = TestTableContext.TestProject1_Models_User;
        table.Alias = "u";
        var ctx = new ResolveContext(CustomOracle.Instance, table);
        var result = exp.Resolve(SqlResolveOptions.Select, ctx);
        Console.WriteLine(result.SqlString);
    }

    [TestMethod]
    public void Abs()
    {
        Expression<Func<User, object>> exp = u => new
        {
            Result = SqlFn.Abs(u.Age)
        };
        var table = TestTableContext.TestProject1_Models_User;
        table.Alias = "u";
        var ctx = new ResolveContext(CustomOracle.Instance, table);
        var result = exp.Resolve(SqlResolveOptions.Select, ctx);
        Console.WriteLine(result.SqlString);
    }
}
