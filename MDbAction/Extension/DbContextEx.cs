using MDbContext;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext {
    public static class DbContextEx {
        public static int Execute(this DbContext self) {
            return self.DbExec.ExcuteNonQuery(self.Sql, self.SqlParameter);
        }

        public static DataTable QueryDataTable(this DbContext self) {
            return self.DbExec.QueryDataTable(self.Sql, self.SqlParameter);
        }

        public static IEnumerable<T> Query<T>(this DbContext self) {
            return self.DbExec.Query<T>(self.Sql, self.SqlParameter);
        }

        public static T Single<T>(this DbContext self) {
            return self.DbExec.SingleResult<T>(self.Sql, self.SqlParameter);
        }

        public static int Execute(this DbContext self, string sql, object p) {
            return self.DbExec.ExcuteNonQuery(sql, p);
        }

        public static DataTable QueryDataTable(this DbContext self, string sql, object p) {
            return self.DbExec.QueryDataTable(sql, p);
        }

        public static IEnumerable<T> Query<T>(this DbContext self, string sql, object p) {
            return self.DbExec.Query<T>(sql, p);
        }

        public static T Single<T>(this DbContext self, string sql, object p) {
            return self.DbExec.SingleResult<T>(sql, p);
        }

        public static void AddTrans(this DbContext self) {
            if (self.TransSqls == null) self.TransSqls = new List<string>();
            if (self.TransSqlParameter == null) self.TransSqlParameter = new List<object>();
            self.TransSqls.Add(self.Sql);
            self.TransSqlParameter.Add(self.SqlParameter);
        }

        public static bool ExecuteTrans(this DbContext self) {
            var sqls = self.TransSqls.ToArray();
            var ps = self.TransSqlParameter.ToArray();
            var result = self.DbExec.ExecuteTransaction(sqls, ps);
            self.TransSqls.Clear();
            self.TransSqlParameter.Clear();
            return result;
        }
    }
}
