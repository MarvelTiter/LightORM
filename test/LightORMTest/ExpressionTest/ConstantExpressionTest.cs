using LightORM.Providers.Sqlite.Extensions;
using LightORM.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace LightORMTest.ExpressionTest;

[TestClass]
public class ConstantExpressionTest
{
    [NotNull] public ResolveContext? Context { get; set; }

    [TestInitialize]
    public void InitResolveContext()
    {
        Context = new(LightORM.Providers.Sqlite.SqliteProvider.Create(o =>
        {
            o.MasterConnectionString = "Data Source=:memory:;Version=3;New=True;";
            o.ConfiguraSqlite(t => t.DetectVersion = false);
        }).DatabaseAdapter);
    }

    [TestMethod]
    public void TestConstantNullValue()
    {
        Expression<Func<User, bool>> exp = u => u.Age == null;
        var result = exp.Resolve(SqlResolveOptions.Where, Context);
        Console.WriteLine(result.SqlString);
        Assert.AreEqual("(`a`.`AGE` IS NULL)", result.SqlString);
    }

    [TestMethod]
    public void TestVaribleValue()
    {
        var resolveOption = SqlResolveOptions.Where;
        var age = 10;
        Expression<Func<User, bool>> exp = u => u.Age > age;
        var result = exp.Resolve(resolveOption, Context);
        Console.WriteLine(result.SqlString);
        Assert.AreEqual("(`a`.`AGE` > age_0_0)", result.SqlString);

        var p = ExpressionValueExtract.Default.Extract(exp, resolveOption, Context);
        Assert.AreEqual("age_0_0", p[0].Name);
    }
}
