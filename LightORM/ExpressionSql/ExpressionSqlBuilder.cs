using MDbContext.ExpressionSql.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

namespace MDbContext.ExpressionSql;

public class ConstString
{
    public const string Main = "MainDb";
}

public class SqlExecuteLife
{
    public Action<string>? BeforeExecute { get; set; }
    internal ExpressionCoreSql? Core { get; set; }
}

public partial class ExpressionSqlBuilder
{
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

    public IExpressionContext Build()
    {
        if ((dbFactories?.Count ?? 0) < 1)
        {
            throw new Exception("未设置连接数据库");
        }
        return new ExpressionCoreSql(dbFactories!, life);
    }
}
