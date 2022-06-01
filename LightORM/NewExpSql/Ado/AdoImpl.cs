using MDbContext.NewExpSql.Interface;
using MDbContext.SqlExecutor;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MDbContext.NewExpSql.Ado
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
            throw new System.NotImplementedException();
        }

        public DataTable ExecuteDataTable(string sql, object param = null)
        {
            var conn = dbFactories[currentDb].CreateConnection();
            return conn.ExecuteTable(sql, param);
        }

        public Task<DataTable> ExecuteDataTableAsync(string sql, object param = null)
        {
            throw new System.NotImplementedException();
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

        public Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<dynamic>> QueryAsync(string sql, object param = null)
        {
            throw new System.NotImplementedException();
        }


        public T Single<T>(string sql, object param = null)
        {
            var conn = dbFactories[currentDb].CreateConnection();
            return conn.QuerySingle<T>(sql, param);
        }

        public Task<T> SingleAsync<T>(string sql, object param = null)
        {
            throw new System.NotImplementedException();
        }
        public IAdo SetDb(string key)
        {
            currentDb = key;
            return this;
        }
    }
}
