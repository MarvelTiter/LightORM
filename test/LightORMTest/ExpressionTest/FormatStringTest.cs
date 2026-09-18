using LightORM.Providers.Sqlite;
using LightORM.Providers.Sqlite.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace LightORMTest.ExpressionTest;

[TestClass]
public class FormatStringTest
{
    [NotNull] public ResolveContext? Context { get; set; }

    [TestInitialize]
    public void InitResolveContext()
    {
        Context = new(SqliteProvider.Create(o =>
        {
            o.MasterConnectionString = "Data Source=:memory:;Version=3;New=True;";
            o.ConfiguraSqlite(t => t.DetectVersion = false);
        }).DatabaseAdapter);
    }
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
        HandleExpressionParameters(Context, exp);
        var result = exp.Resolve(SqlResolveOptions.Where, Context);
        Console.WriteLine(result.SqlString);
    }

    [TestMethod]
    public void InterpolationFormatOption()
    {
        Expression<Func<User, bool>> exp = u => u.UserName == $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        HandleExpressionParameters(Context, exp);
        var result = exp.Resolve(SqlResolveOptions.Where, Context);
        Console.WriteLine(result.SqlString);
    }
}
