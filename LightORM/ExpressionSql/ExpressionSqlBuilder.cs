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
    // internal ConcurrentDictionary<string, DbConnectInfo> DbFactories { get; } = new ConcurrentDictionary<string, DbConnectInfo>();
    public ExpressionSqlOptions SetDatabase(DbBaseType dbBaseType, string connStr, DbProviderFactory factory)
    {
        return SetDatabase(ConstString.Main, dbBaseType, connStr, factory);
    }
    public ExpressionSqlOptions SetDatabase(string key, DbBaseType dbBaseType, string connStr, DbProviderFactory factory)
    {
        var info = new DbConnectInfo(dbBaseType, connStr, factory);
        _ = StaticCache<DbConnectInfo>.GetOrAdd(key, () => info);
        return this;
    }
    
    internal SqlAopProvider Aop { get; } = new SqlAopProvider();
    public ExpressionSqlOptions SetWatcher(Action<SqlAopProvider> option)
    {
        option(Aop);
        return this;
    }
    internal DbInitialContext? ContextInitializer { get; set; }
   

    public ExpressionSqlOptions InitializedContext<T>(Action<DbInfo>? option = null) where T : DbInitialContext, new()
    {
        DbInfo info = new DbInfo();
        option?.Invoke(info);
        ContextInitializer = new T();
        ContextInitializer.Info = info;
        return this;
    }
}

public partial class ExpressionSqlBuilder
{
    private readonly ExpressionSqlOptions options;

    public ExpressionSqlBuilder(ExpressionSqlOptions options)
    {
        this.options = options;
    }
    
    internal IExpressionContext InnerBuild()
    {
        return new ExpressionCoreSql(options.Aop);
    }

    public IExpressionContext Build()
    {
        var context = (InnerBuild() as ExpressionCoreSql)!;

        options.ContextInitializer?.Check(context);
        return context;
    }

#if NET6_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
    public IExpressionContext Build(IServiceProvider provider)
    {
        var context = (InnerBuild() as ExpressionCoreSql)!;
        if (options.ContextInitializer != null)
        {
            var logger = provider?.GetService(typeof(Microsoft.Extensions.Logging.ILogger<IExpressionContext>)) as Microsoft.Extensions.Logging.ILogger<IExpressionContext>;
            context.Logger = logger;
            //var instance = Activator.CreateInstance(options.ContextInitialType);
            //var methodExp = Expression.Call(Expression.Constant(instance)
            //    , ExpressionContext.InitializedMethod
            //    , Expression.Convert(Expression.Constant(context), typeof(IDbInitial)));
            //Expression.Lambda(methodExp).Compile().DynamicInvoke();
            options.ContextInitializer.Check(context);
        }
        return context;
    }
#endif
}
