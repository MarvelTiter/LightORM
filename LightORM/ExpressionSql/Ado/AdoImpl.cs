using MDbContext;
using MDbContext.ExpressionSql.Interface;
using MDbContext.SqlExecutor;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Ado
{
    public class AdoImpl : IAdo
    {
        private string currentDb = "MainDb";
        private readonly ConcurrentDictionary<string, DbConnectInfo> dbFactories;
        internal AdoImpl(ConcurrentDictionary<string, DbConnectInfo> dbFactories)
        {
            this.dbFactories = dbFactories;
        }
        public int Execute(string sql, object param = null)
        {
            var conn = dbFactories[currentDb].CreateConnection();
            return conn.Execute(sql, param);
        }

        public Task<int> ExecuteAsync(string sql, object param = null)
        {
            var conn = dbFactories[currentDb].CreateConnection();
            return conn.ExecuteAsync(sql, param);
        }

        public DataTable ExecuteDataTable(string sql, object param = null)
        {
            var conn = dbFactories[currentDb].CreateConnection();
            return conn.ExecuteTable(sql, param);
        }

        public Task<DataTable> ExecuteDataTableAsync(string sql, object param = null)
        {
            var conn = dbFactories[currentDb].CreateConnection();
            return conn.ExecuteTableAsync(sql, param);
        }

        public IEnumerable<T> Query<T>(string sql, object param = null)
        {
            var conn = dbFactories[currentDb].CreateConnection();
            return conn.Query<T>(sql, param);
        }

        public IEnumerable<dynamic> Query(string sql, object param = null)
        {
            var conn = dbFactories[currentDb].CreateConnection();
            return conn.Query(sql, param);
        }

        public Task<List<T>> QueryAsync<T>(string sql, object param = null)
        {
            var conn = dbFactories[currentDb].CreateConnection();
            return conn.QueryAsync<T>(sql, param);
        }

        public Task<IEnumerable<dynamic>> QueryAsync(string sql, object param = null)
        {
            var conn = dbFactories[currentDb].CreateConnection();
            return conn.QueryAsync(sql, param);
        }


        public T Single<T>(string sql, object param = null)
        {
            var conn = dbFactories[currentDb].CreateConnection();
            return conn.QuerySingle<T>(sql, param);
        }

        public Task<T> SingleAsync<T>(string sql, object param = null)
        {
            var conn = dbFactories[currentDb].CreateConnection();
            return conn.QuerySingleAsync<T>(sql, param);
        }
        public IAdo SetDb(string key)
        {
            if (!dbFactories.ContainsKey(key))
                throw new System.ArgumentException($"未注册的数据库:{key}");
            currentDb = key;
            return this;
        }
    }
}
