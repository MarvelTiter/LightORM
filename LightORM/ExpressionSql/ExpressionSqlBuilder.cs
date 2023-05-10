using MDbContext.ExpressionSql.Interface;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace MDbContext.ExpressionSql;

public enum SqlAction
{
    Select,
    Update,
    Insert,
    Delete,
}

public class ConstString
{
    public const string Main = "MainDb";
}
public class SqlArgs : EventArgs
{
    public object? SqlParameter { get; set; }
    public SqlAction Action { get; set; }
    public string? Sql { get; set; }
    public bool Done { get; set; } = false;
}
public class SqlExecuteLife
{
    public Action<SqlArgs>? BeforeExecute { get; set; }
    public Action<SqlArgs>? AfterExecute { get; set; }
    internal ExpressionCoreSql? Core { get; set; }
}

public class ExpressionSqlOptions
{
    internal ConcurrentDictionary<string, DbConnectInfo> DbFactories { get; } = new ConcurrentDictionary<string, DbConnectInfo>();
    public ExpressionSqlOptions SetDatabase(DbBaseType dbBaseType, Func<IDbConnection> dbFactory)
    {
        DbFactories[ConstString.Main] = new DbConnectInfo() { DbBaseType = dbBaseType, CreateConnection = dbFactory };
        return this;
    }
    public ExpressionSqlOptions SetSalveDatabase(string key, DbBaseType dbBaseType, Func<IDbConnection> dbFactory)
    {
        if (key == ConstString.Main) throw new ArgumentException("key 不能为 MainDb");
        DbFactories[key] = new DbConnectInfo() { DbBaseType = dbBaseType, CreateConnection = dbFactory };
        return this;
    }
    internal SqlExecuteLife Life { get; } = new SqlExecuteLife();
    public ExpressionSqlOptions SetWatcher(Action<SqlExecuteLife> option)
    {
        option(Life);
        return this;
    }
    internal Type? ContextInitialType { get; set; }
    //public ExpressionSqlOptions InitializedContext(Type type)
    //{
    //    this.ContextInitialType = type;
    //    return this;
    //}
    public ExpressionSqlOptions InitializedContext<T>() where T : ExpressionContext, new()
    {
        this.ContextInitialType = typeof(T);
        return this;
    }
}

public partial class ExpressionSqlBuilder
{
    private readonly ExpressionSqlOptions options;

    ConcurrentDictionary<string, DbConnectInfo> dbFactories = new ConcurrentDictionary<string, DbConnectInfo>();
    public ExpressionSqlBuilder SetDatabase(DbBaseType dbBaseType, Func<IDbConnection> dbFactory)
    {
        dbFactories[ConstString.Main] = new DbConnectInfo() { DbBaseType = dbBaseType, CreateConnection = dbFactory };
        return this;
    }
    public ExpressionSqlBuilder SetSalveDatabase(string key, DbBaseType dbBaseType, Func<IDbConnection> dbFactory)
    {
        if (key == ConstString.Main) throw new ArgumentException("key 不能为 MainDb");
        dbFactories[key] = new DbConnectInfo() { DbBaseType = dbBaseType, CreateConnection = dbFactory };
        return this;
    }
    SqlExecuteLife life = new SqlExecuteLife();
    public ExpressionSqlBuilder SetWatcher(Action<SqlExecuteLife> option)
    {
        option(life);
        return this;
    }
    public ExpressionSqlBuilder()
    {

    }
    public ExpressionSqlBuilder(ExpressionSqlOptions options)
    {
        this.options = options;
    }
    [Obsolete]
    public IExpressionContext BuildContext()
    {
        if ((dbFactories?.Count ?? 0) < 1)
        {
            throw new Exception("未设置连接数据库");
        }
        return new ExpressionCoreSql(dbFactories!, life);
    }

    public IExpressionContext Build()
    {
        if ((options.DbFactories?.Count ?? 0) < 1)
        {
            throw new Exception("未设置连接数据库");
        }
        return new ExpressionCoreSql(options.DbFactories!, options.Life);
    }
#if NET6_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
    public IExpressionContext Build(IServiceProvider provider)
    {
        var context = (Build() as ExpressionCoreSql)!;
        if (options.ContextInitialType != null)
        {
            var logger = provider?.GetService(typeof(Microsoft.Extensions.Logging.ILogger<IExpressionContext>)) as Microsoft.Extensions.Logging.ILogger<IExpressionContext>;
            context.Logger = logger;
            var instance = Activator.CreateInstance(options.ContextInitialType);
            var methodExp = Expression.Call(Expression.Constant(instance)
                , ExpressionContext.InitializedMethod
                , Expression.Convert(Expression.Constant(context), typeof(IDbInitial)));
            Expression.Lambda(methodExp).Compile().DynamicInvoke();
        }
        return context;
    }
#endif
}
