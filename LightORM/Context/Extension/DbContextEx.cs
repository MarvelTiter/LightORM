using MDbContext.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MDbContext
{
    public static class DbContextEx
    {
        private static Task<T> RunAsync<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    var result = func.Invoke();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }
        public static int Execute(this DbContext self) => self.DbConnection.Execute(self.Sql, self.SqlParameter);
        public static Task<int> ExecuteAsync(this DbContext self)
        {
            return RunAsync(() =>
            {
                return self.Execute();
            });
        }

        public static DataTable QueryDataTable(this DbContext self) => self.DbConnection.ExecuteTable(self.Sql, self.SqlParameter);
        public static Task<DataTable> QueryDataTableAsync(this DbContext self)
        {
            return RunAsync(() =>
            {
                return self.QueryDataTable();
            });
        }

        public static IEnumerable<T> Query<T>(this DbContext self) => self.DbConnection.Query<T>(self.Sql, self.SqlParameter);

        public static Task<IEnumerable<T>> QueryAsync<T>(this DbContext self)
        {
            return RunAsync<IEnumerable<T>>(() =>
            {
                return self.Query<T>().ToList();
            });
        }
        public static T Single<T>(this DbContext self) => self.DbConnection.QuerySingle<T>(self.Sql, self.SqlParameter);
        public static Task<T> SingleAsync<T>(this DbContext self)
        {
            return RunAsync(() =>
            {
                return self.Single<T>();
            });
        }



        public static int Execute(this DbContext self, string sql, object p) => self.DbConnection.Execute(sql, p);
        public static Task<int> ExecuteAsync(this DbContext self, string sql, object p)
        {
            return RunAsync(() =>
            {
                return self.Execute(sql, p);
            });
        }

        public static DataTable QueryDataTable(this DbContext self, string sql, object p) => self.DbConnection.ExecuteTable(sql, p);
        public static Task<DataTable> QueryDataTableAsync(this DbContext self, string sql, object p)
        {
            return RunAsync(() =>
            {
                return self.QueryDataTable(sql, p);
            });
        }

        public static IEnumerable<T> Query<T>(this DbContext self, string sql, object p) => self.DbConnection.Query<T>(sql, p);
        public static Task<IEnumerable<T>> QueryAsync<T>(this DbContext self, string sql, object p)
        {
            return RunAsync<IEnumerable<T>>(() =>
            {
                return self.Query<T>(sql, p).ToList();
            });
        }

        public static T Single<T>(this DbContext self, string sql, object p) => self.DbConnection.QuerySingle<T>(sql, p);
        public static Task<T> SingleAsync<T>(this DbContext self, string sql, object p)
        {
            return RunAsync(() =>
            {
                return self.Single<T>(sql, p);
            });
        }
        public static Task ExecuteDataReaderAsync(this DbContext self, Func<IDataReader, Task> taskFunc) => ExecuteDataReaderAsync(self, self.Sql, self.SqlParameter, taskFunc);
        public static async Task ExecuteDataReaderAsync(this DbContext self, string sql, object p, Func<IDataReader, Task> taskFunc)
        {
            var reader = self.DbConnection.ExecuteReader(sql, p);
            await taskFunc(reader);
            reader.Close();
        }

        public static void AddTrans(this DbContext self)
        {
            if (self.TransSqls == null) self.TransSqls = new List<string>();
            if (self.TransSqlParameter == null) self.TransSqlParameter = new List<object>();
            self.TransSqls.Add(self.Sql);
            self.TransSqlParameter.Add(self.SqlParameter);
        }
        public static void AddTrans(this DbContext self, string sql, object param)
        {
            if (self.TransSqls == null) self.TransSqls = new List<string>();
            if (self.TransSqlParameter == null) self.TransSqlParameter = new List<object>();
            self.TransSqls.Add(sql);
            self.TransSqlParameter.Add(param);
        }
        public static bool ExecuteTrans(this DbContext self)
        {
            var sqls = self.TransSqls;
            var ps = self.TransSqlParameter;
            var result = self.DbConnection.ExecuteTrans(sqls, ps);
            self.TransSqls.Clear();
            self.TransSqlParameter.Clear();
            return result;
        }
        public static Task<bool> ExecuteTransAsync(this DbContext self)
        {
            return RunAsync(() =>
            {
                return self.ExecuteTrans();
            });
        }
    }
}
