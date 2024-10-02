namespace LightORM;

internal class ConstString
{
    public const string Main = "MainDb";
}

public class SqlAopProvider
{
    public Action<string, object?>? DbLog { get; set; }
}

internal record DbHandlerRecord(Func<TableGenerateOption, IDatabaseTableHandler>? Factory);

public class ExpressionSqlOptions
{
    public ExpressionSqlOptions SetDatabase(string? key, DbBaseType dbBaseType, IDatabaseProvider provider)
    {
        var k = key ?? ConstString.Main;
        if (StaticCache<IDatabaseProvider>.HasKey(k))
        {
            LightOrmException.Throw($"SetDatabase 设置了重复的Key => {key}");
        }
        //var info = new DbConnectInfo(dbBaseType, provider);
        _ = StaticCache<IDatabaseProvider>.GetOrAdd(k, () => provider);
        //_ = StaticCache<IDatabaseProvider>.GetOrAdd()
        if (!StaticCache<ICustomDatabase>.HasKey(dbBaseType.Name))
        {
            StaticCache<ICustomDatabase>.GetOrAdd(dbBaseType.Name, () => provider.CustomDatabase);
        }
        if (!StaticCache<DbHandlerRecord>.HasKey(dbBaseType.Name))
        {
            StaticCache<DbHandlerRecord>.GetOrAdd(dbBaseType.Name, () => new DbHandlerRecord(provider.TableHandler));
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
