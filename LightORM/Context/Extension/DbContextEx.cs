using MDbContext.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MDbContext {
    public static class DbContextEx {
        public static int Execute(this DbContext self) => self.DbConnection.Execute(self.Sql, self.SqlParameter);
        public static Task<int> ExecuteAsync(this DbContext self) {
            TaskCompletionSource<int> task = new TaskCompletionSource<int>();
            try {
                var result = self.Execute();
                task.SetResult(result);
            } catch (Exception ex) {
                task.SetException(ex);
            }
            return task.Task;
        }

        public static DataTable QueryDataTable(this DbContext self) => self.DbConnection.ExecuteReader(self.Sql, self.SqlParameter);
        public static Task<DataTable> QueryDataTableAsync(this DbContext self) {
            TaskCompletionSource<DataTable> task = new TaskCompletionSource<DataTable>();
            try {
                var dt = self.QueryDataTable();
                task.SetResult(dt);
            } catch (Exception ex) {
                task.SetException(ex);
            }
            return task.Task;
        }

        public static IEnumerable<T> Query<T>(this DbContext self) => self.DbConnection.Query<T>(self.Sql, self.SqlParameter);
        public static Task<IEnumerable<T>> QueryAsync<T>(this DbContext self) {
            TaskCompletionSource<IEnumerable<T>> task = new TaskCompletionSource<IEnumerable<T>>();
            try {
                var result = self.Query<T>();
                task.SetResult(result);
            } catch (Exception ex) {
                task.SetException(ex);
            }
            return task.Task;
        }

        public static T Single<T>(this DbContext self) => self.DbConnection.QuerySingle<T>(self.Sql, self.SqlParameter);
        public static Task<T> SingleAsync<T>(this DbContext self) {
            TaskCompletionSource<T> task = new TaskCompletionSource<T>();
            try {
                var result = self.Single<T>();
                task.SetResult(result);
            } catch (Exception ex) {
                task.SetException(ex);
            }
            return task.Task;
        }

        public static int Execute(this DbContext self, string sql, object p) => self.DbConnection.Execute(sql, p);
        public static Task<int> ExecuteAsync(this DbContext self, string sql, object p) {
            TaskCompletionSource<int> task = new TaskCompletionSource<int>();
            try {
                var result = self.Execute(sql, p);
                task.SetResult(result);
            } catch (Exception ex) {
                task.SetException(ex);
            }
            return task.Task;
        }

        public static DataTable QueryDataTable(this DbContext self, string sql, object p) => self.DbConnection.ExecuteReader(sql, p);
        public static Task<DataTable> QueryDataTableAsync(this DbContext self, string sql, object p) {
            TaskCompletionSource<DataTable> task = new TaskCompletionSource<DataTable>();
            try {
                var dt = self.QueryDataTable(sql, p);
                task.SetResult(dt);
            } catch (Exception ex) {
                task.SetException(ex);
            }
            return task.Task;
        }

        public static IEnumerable<T> Query<T>(this DbContext self, string sql, object p) => self.DbConnection.Query<T>(sql, p);
        public static Task<IEnumerable<T>> QueryAsync<T>(this DbContext self, string sql, object p) {
            TaskCompletionSource<IEnumerable<T>> task = new TaskCompletionSource<IEnumerable<T>>();
            try {
                var result = self.Query<T>(sql, p);
                task.SetResult(result);
            } catch (Exception ex) {
                task.SetException(ex);
            }
            return task.Task;
        }

        public static T Single<T>(this DbContext self, string sql, object p) => self.DbConnection.QuerySingle<T>(sql, p);
        public static Task<T> SingleAsync<T>(this DbContext self, string sql, object p) {
            TaskCompletionSource<T> task = new TaskCompletionSource<T>();
            try {
                var result = self.Single<T>(sql, p);
                task.SetResult(result);
            } catch (Exception ex) {
                task.SetException(ex);
            }
            return task.Task;
        }

        public static void AddTrans(this DbContext self) {
            if (self.TransSqls == null) self.TransSqls = new List<string>();
            if (self.TransSqlParameter == null) self.TransSqlParameter = new List<object>();
            self.TransSqls.Add(self.Sql);
            self.TransSqlParameter.Add(self.SqlParameter);
        }
        public static void AddTrans(this DbContext self, string sql, object param) {
            if (self.TransSqls == null) self.TransSqls = new List<string>();
            if (self.TransSqlParameter == null) self.TransSqlParameter = new List<object>();
            self.TransSqls.Add(sql);
            self.TransSqlParameter.Add(param);
        }
        public static bool ExecuteTrans(this DbContext self) {
            var sqls = self.TransSqls;
            var ps = self.TransSqlParameter;
            var result = self.DbConnection.ExecuteTrans(sqls, ps);
            self.TransSqls.Clear();
            self.TransSqlParameter.Clear();
            return result;
        }
        public static Task<bool> ExecuteTransAsync(this DbContext self) {
            TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
            try {
                var result = self.ExecuteTrans();
                task.SetResult(result);
            } catch (Exception ex) {
                task.SetException(ex);
            }
            return task.Task;
        }
    }
}
