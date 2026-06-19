using LightORM.Providers.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;

namespace LightORMTest.ExpressionTest;

[TestClass]
public class FormatStringTest 
{
    private TestTableContext TableContext { get; set; } = new();
    [TestMethod]
    public void InterpolationFormat()
    {
        var name = "test";
        var seq = 0;
        Expression<Func<User, bool>> exp = u => u.UserName == $"{name}{seq}";
        var table = TableContext.GetTableInfo(typeof(User))!;
        var ctx = new ResolveContext(CustomSqliteAdapter.TestInstance, table);
        var result = exp.Resolve(SqlResolveOptions.Where, ctx);
        Console.WriteLine(result.SqlString);
    }

    [TestMethod]
    public void InterpolationFormatOption()
    {
        Expression<Func<User, bool>> exp = u => u.UserName == $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        var table = TableContext.GetTableInfo(typeof(User))!;
        var ctx = new ResolveContext(CustomSqliteAdapter.TestInstance, table);
        var result = exp.Resolve(SqlResolveOptions.Where, ctx);
        Console.WriteLine(result.SqlString);
    }
}
