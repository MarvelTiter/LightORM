using System.Data.Common;
using LightORM.Cache;
using LightORM.ExpressionSql;

namespace LightORM;

internal class ConstString
{
    public const string Main = "MainDb";
}

public class SqlAopProvider
{
    public Action<string, object?>? DbLog { get; set; }
}

public class ExpressionSqlOptions
{
    public ExpressionSqlOptions SetDatabase(string? key, DbBaseType dbBaseType, IDatabaseProvider provider)
    {
        var info = new DbConnectInfo(dbBaseType, provider);
        _ = StaticCache<DbConnectInfo>.GetOrAdd(key ?? ConstString.Main, () => info);
        //_ = StaticCache<IDatabaseProvider>.GetOrAdd()
        return this;
    }
    public ExpressionSqlOptions AddDatabaseHandler(DbBaseType dbBaseType, Func<TableGenerateOption, IDatabaseTableHandler> factory)
    {
        return this;
    }
    public ExpressionSqlOptions AddDatabaseCustomer(DbBaseType dbBaseType, ICustomDatabase customDatabase)
    {
        if (!StaticCache<ICustomDatabase>.HasKey(dbBaseType.Name))
        {
            StaticCache<ICustomDatabase>.GetOrAdd(dbBaseType.Name, () => customDatabase);
        }
        return this;
    }

    public ExpressionSqlOptions SetDatabase(DbBaseType dbBaseType, IDatabaseProvider provider) => SetDatabase(null, dbBaseType, provider);

    public ExpressionSqlOptions SetTableContext(ITableContext context)
    {
        TableContext.StaticContext = context;
        return this;
    }

    internal SqlAopProvider Aop { get; } = new SqlAopProvider();
    public ExpressionSqlOptions SetWatcher(Action<SqlAopProvider> option)
    {
        option(Aop);
        return this;
    }
    internal List<DbInitialContext> InitialContexts { get; set; } = [];
    public ExpressionSqlOptions InitializedContext<T>(Action<DbInfo>? option = null) where T : DbInitialContext, new()
    {
        DbInfo info = new DbInfo();
        option?.Invoke(info);
        var ctx = new T();
        ctx.Info = info;
        InitialContexts.Add(ctx);
        return this;
    }

    internal void Check()
    {
        foreach (var ctx in InitialContexts)
        {
            ctx.Check(this);
        }
    }
}

public partial class ExpressionSqlBuilder
{
    private readonly ExpressionSqlOptions options;

    public ExpressionSqlBuilder(ExpressionSqlOptions options)
    {
        // TODO数据库初始化
        this.options = options;
    }

    internal IExpressionContext InnerBuild()
    {
        return new ExpressionCoreSql(options);
    }

    public IExpressionContext Build()
    {
        //var context = (InnerBuild() as ExpressionCoreSql)!;

        ////options.ContextInitializer?.Check(context);
        //return context;
        return InnerBuild();
    }
}
