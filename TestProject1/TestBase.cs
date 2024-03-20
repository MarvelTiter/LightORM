using System.Linq.Expressions;
using LightORM.ExpressionSql;
using LightORM.Interfaces;
using LightORM.Models;
using LightORM.Utils;
using Microsoft.Data.Sqlite;
namespace TestProject1;

public class TestBase
{
    protected IExpressionContext Context { get; }
    public TestBase()
    {
        var options = new ExpressionSqlOptions();
        var path = Path.GetFullPath("../../../Demo.db");
        options.SetDatabase(LightORM.Context.DbBaseType.Sqlite, "DataSource=" + path, SqliteFactory.Instance);
        var builder = new ExpressionSqlBuilder(options);
        Context = builder.Build();
    }
}