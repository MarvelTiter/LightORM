using MDbContext.SqlExecutor;
using System.Collections.Generic;
using System.Data;

namespace MDbContext {
    public static class DbContextEx {
        public static int Execute(this DbContext self) => self.DbConnection.Execute(self.Sql, self.SqlParameter);

        public static DataTable QueryDataTable(this DbContext self) => self.DbConnection.ExecuteReader(self.Sql, self.SqlParameter);

        public static IEnumerable<T> Query<T>(this DbContext self) => self.DbConnection.Query<T>(self.Sql, self.SqlParameter);

        public static T Single<T>(this DbContext self) => self.DbConnection.QuerySingle<T>(self.Sql, self.SqlParameter);

        public static int Execute(this DbContext self, string sql, object p) => self.DbConnection.Execute(sql, p);

        public static DataTable QueryDataTable(this DbContext self, string sql, object p) => self.DbConnection.ExecuteReader(sql, p);

        public static IEnumerable<T> Query<T>(this DbContext self, string sql, object p) => self.DbConnection.Query<T>(sql, p);

        public static T Single<T>(this DbContext self, string sql, object p) => self.DbConnection.QuerySingle<T>(sql, p);

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
    }
}
