using MDbContext;
using MDbContext.ExpressionSql.Interface;
using MDbContext.SqlExecutor;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System;
namespace MDbContext.ExpressionSql.Ado
{
    public class AdoImpl : IAdo
    {
        private string? current = null;
        private const string MAIN = "MainDb";
        private readonly ConcurrentDictionary<string, DbConnectInfo> dbFactories;
        internal AdoImpl(ConcurrentDictionary<string, DbConnectInfo> dbFactories)
        {
            this.dbFactories = dbFactories;
        }

        IDbConnection CurrentConnection
        {
            get
            {
                var k = current ?? MAIN;
                current = null;
                if (dbFactories.TryGetValue(k, out var conn))
                {
                    return conn.CreateConnection();
                }
                throw new ArgumentException($"未注册的数据库:{k}");
            }
        }

        public int Execute(string sql, object param = null)
        {
            return CurrentConnection.Execute(sql, param);
        }

        public Task<int> ExecuteAsync(string sql, object param = null)
        {
            return CurrentConnection.ExecuteAsync(sql, param);
        }

        public DataTable ExecuteDataTable(string sql, object param = null)
        {
            return CurrentConnection.ExecuteTable(sql, param);
        }

        public Task<DataTable> ExecuteDataTableAsync(string sql, object param = null)
        {
            return CurrentConnection.ExecuteTableAsync(sql, param);
        }

        public IEnumerable<T> Query<T>(string sql, object param = null)
        {
            return CurrentConnection.Query<T>(sql, param);
        }

        public IEnumerable<dynamic> Query(string sql, object param = null)
        {
            return CurrentConnection.Query(sql, param);
        }

        public Task<List<T>> QueryAsync<T>(string sql, object param = null)
        {
            return CurrentConnection.QueryAsync<T>(sql, param);
        }

        public Task<IEnumerable<dynamic>> QueryAsync(string sql, object param = null)
        {
            return CurrentConnection.QueryAsync(sql, param);
        }

        public T Single<T>(string sql, object param = null)
        {
            return CurrentConnection.QuerySingle<T>(sql, param);
        }

        public Task<T> SingleAsync<T>(string sql, object param = null)
        {
            return CurrentConnection.QuerySingleAsync<T>(sql, param);
        }

        public IAdo SwitchDatabase(string key)
        {
            if (!dbFactories.ContainsKey(key))
                throw new ArgumentException($"未注册的数据库:{key}");
            current = key;
            return this;
        }
    }
}
