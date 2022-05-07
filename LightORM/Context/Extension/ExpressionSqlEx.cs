using DExpSql;
using MDbContext.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.Context.Extension
{
    public static class ExpressionSqlEx
    {
        public static Task<IEnumerable<T>> ToListAsync<T>(this ExpressionSqlCore self)
        {
            var db = self.GetDbContext();
            Console.WriteLine($"==================={DateTime.Now} Sql===================");
            Console.WriteLine(db.DbSet);
            return db.QueryAsync<T>();
        }

        public static async Task<IEnumerable<dynamic>> ToListAsync(this ExpressionSqlCore self)
        {
            var db = self.GetDbContext();
            Console.WriteLine($"==================={DateTime.Now} Sql===================");
            Console.WriteLine(db.DbSet);
            return await db.QueryAsync<MapperRow>();
        }

        public static Task<DataTable> ToDataTableAsync(this ExpressionSqlCore self)
        {
            var db = self.GetDbContext();
            Console.WriteLine($"==================={DateTime.Now} Sql===================");
            Console.WriteLine(db.DbSet);
            return db.QueryDataTableAsync();
        }

        public static Task<T> FirstAsync<T>(this ExpressionSqlCore self)
        {
            var db = self.GetDbContext();
            Console.WriteLine($"==================={DateTime.Now} Sql===================");
            Console.WriteLine(db.DbSet);
            return db.SingleAsync<T>();
        }
    }
}
