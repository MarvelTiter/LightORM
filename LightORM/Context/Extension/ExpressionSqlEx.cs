using DExpSql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.Context.Extension
{
    public static class ExpressionSqlEx
    {
        public static Task<IEnumerable<T>> ToList<T>(this ExpressionSqlCore self)
        {
            var db = self.GetDbContext();
            return db.QueryAsync<T>();
        }

        public static Task<DataTable> ToDataTable(this ExpressionSqlCore self)
        {
            var db = self.GetDbContext();
            return db.QueryDataTableAsync();
        }

        public static Task<T> First<T>(this ExpressionSqlCore self)
        {
            var db = self.GetDbContext();
            return db.SingleAsync<T>();
        }
    }
}
