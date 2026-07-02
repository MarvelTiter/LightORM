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
    private static void HandleExpressionParameters(ResolveContext context, LambdaExpression lambda)
    {
        for (int i = 0; i < lambda.Parameters.Count; i++)
        {
            ParameterExpression? item = lambda.Parameters[i];
            context.HandleParameterExpression(item, i);
        }
    }
    private TestTableContext TableContext { get; set; } = new();
    [TestMethod]
    public void InterpolationFormat()
    {
        var name = "test";
        var seq = 0;
        Expression<Func<User, bool>> exp = u => u.UserName == $"{name}{seq}";
        var ctx = new ResolveContext(CustomSqliteAdapter.TestInstance);
        HandleExpressionParameters(ctx, exp);
        var result = exp.Resolve(SqlResolveOptions.Where, ctx);
        Console.WriteLine(result.SqlString);
    }

    [TestMethod]
    public void InterpolationFormatOption()
    {
        Expression<Func<User, bool>> exp = u => u.UserName == $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        var ctx = new ResolveContext(CustomSqliteAdapter.TestInstance);
        HandleExpressionParameters(ctx, exp);
        var result = exp.Resolve(SqlResolveOptions.Where, ctx);
        Console.WriteLine(result.SqlString);
    }
}
