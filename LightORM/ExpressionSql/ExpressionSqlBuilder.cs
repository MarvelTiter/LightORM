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
