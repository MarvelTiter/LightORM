#if NET40
#else
using MDbContext.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Providers
{
    internal partial class BasicProvider<T1>
    {
        internal async Task<TReturn> InternalSingleAsync<TReturn>(SqlArgs args)
        {
            Life.BeforeExecute?.Invoke(args);
            using var conn = dbConnect.CreateConnection();
            var ret = await conn.QuerySingleAsync<TReturn>(args.Sql!, args.SqlParameter);
            args.Done = true;
            Life.AfterExecute?.Invoke(args);
            return ret;
        }
        internal async Task<int> InternalExecuteAsync(SqlArgs args)
        {
            Life.BeforeExecute?.Invoke(args);
            using var conn = dbConnect.CreateConnection();
            var ret = await conn.ExecuteAsync(args.Sql!, args.SqlParameter);
            args.Done = true;
            Life.AfterExecute?.Invoke(args);
            return ret;
        }
        internal async Task<IList<TReturn>> InternalQueryAsync<TReturn>(SqlArgs args)
        {
            Life.BeforeExecute?.Invoke(args);
            var conn = dbConnect.CreateConnection();
            var ret = await conn.QueryAsync<TReturn>(args.Sql!, args.SqlParameter);
            args.Done = true;
            Life.AfterExecute?.Invoke(args);
            return ret;
        }
        internal async Task<IList<dynamic>> InternalQueryAsync(SqlArgs args)
        {
            Life.BeforeExecute?.Invoke(args);
            using var conn = dbConnect.CreateConnection();
            var list = await conn.QueryAsync(args.Sql!, args.SqlParameter);
            args.Done = true;
            Life.AfterExecute?.Invoke(args);
            return list.ToList();
        }
    }
}
#endif
