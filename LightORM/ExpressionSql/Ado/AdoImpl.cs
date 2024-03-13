using System.Collections.Generic;
using System.Data;
using LightORM.ExpressionSql.Interface;

namespace LightORM.ExpressionSql.Ado;

public partial class AdoImpl : IAdo
{
    //private string? current = null;
    //private const string MAIN = "MainDb";
    //private readonly ConcurrentDictionary<string, DbConnectInfo> dbFactories;
    private readonly DbConnectInfo connectInfo;
    private readonly SqlExecuteLife life;

    //internal AdoImpl(ConcurrentDictionary<string, DbConnectInfo> dbFactories)
    //{
    //    this.dbFactories = dbFactories;
    //}

    internal AdoImpl(DbConnectInfo connectInfo, SqlExecuteLife life)
    {
        this.connectInfo = connectInfo;
        this.life = life;
    }

    IDbConnection CurrentConnection
    {
        get
        {
            //var k = current ?? MAIN;
            //current = null;
            //if (dbFactories.TryGetValue(k, out var conn))
            //{
            //    return conn.CreateConnection();
            //}
            //throw new ArgumentException($"未注册的数据库:{k}");
            return connectInfo.CreateConnection();
        }
    }

    T InternalExecute<T>(string sql, object? param, Func<T> executor)
    {
        SqlArgs args = new SqlArgs() { Action = SqlAction.ExecuteSql, Sql = sql, SqlParameter = param };
        life.BeforeExecute?.Invoke(args);
        var result = executor();
        life.AfterExecute?.Invoke(args);
        return result;
    }

    public int Execute(string sql, object? param = null)
    {
        return InternalExecute(sql, param, () =>
         {
             return CurrentConnection.Execute(sql, param);
         });
    }

    public DataTable ExecuteDataTable(string sql, object? param = null)
    {
        return InternalExecute(sql, param, () =>
        {
            return CurrentConnection.ExecuteTable(sql, param);
        });
    }

    public IEnumerable<T> Query<T>(string sql, object? param = null)
    {
        return InternalExecute(sql, param, () =>
        {
            return CurrentConnection.Query<T>(sql, param);
        });
    }

    public IEnumerable<dynamic> Query(string sql, object? param = null)
    {
        return InternalExecute(sql, param, () =>
        {
            return CurrentConnection.Query(sql, param);
        });
    }

    public T? Single<T>(string sql, object? param = null)
    {
        return InternalExecute(sql, param, () =>
        {
            return CurrentConnection.QuerySingle<T>(sql, param);
        });
    }

    //public IAdo SwitchDatabase(string key)
    //{
    //    if (!dbFactories.ContainsKey(key))
    //        throw new ArgumentException($"未注册的数据库:{key}");
    //    current = key;
    //    return this;
    //}

    public void Query(string sql, object? param, Action<IDataReader> callback)
    {
        InternalExecute<int>(sql, param, () =>
       {
           CurrentConnection.ExecuteReader(sql, param, callback);
           return 0;
       });
    }

}
