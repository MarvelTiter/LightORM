using MDbContext.ExpressionSql.Interface.Select;
using MDbContext.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Providers.Select;

internal partial class BasicSelect0<TSelect, T1> : BasicProvider<T1>, IExpSelect0<TSelect, T1> where TSelect : class, IExpSelect0
{
    public Task<IList<T1>> ToListAsync(Expression<Func<T1, object>> exp)
    {
        SelectHandle(exp.Body);
        var args = BuildArgs();
        return InternalQueryAsync<T1>(args);
    }
    public Task<IList<T1>> ToListAsync()
    {
        SelectHandle(selectBody);
        var args = BuildArgs();
        return InternalQueryAsync<T1>(args);
    }
    public Task<IList<dynamic>> ToDynamicListAsync()
    {
        SelectHandle(selectBody);
        var args = BuildArgs();
        return InternalQueryAsync(args);
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>()
    {
        //Expression<Func<TReturn, object>> exp = r => new { r };
        //SelectHandle(exp.Body);
        SelectHandle(selectBody);
        var args = BuildArgs();
        return InternalQueryAsync<TReturn>(args);
    }
    public IEnumerable<T1> ToList()
    {
        SelectHandle(selectBody);
        var args = BuildArgs();
        return InternalQuery<T1>(args);
    }
    public IEnumerable<dynamic> ToDynamicList()
    {
        SelectHandle(selectBody);
        var args = BuildArgs();
        return InternalQuery(args);
    }
    public IEnumerable<TReturn> ToList<TReturn>()
    {
        //Expression<Func<TReturn, object>> exp = r => new { r };
        SelectHandle(selectBody);
        var args = BuildArgs();
        return InternalQuery<TReturn>(args);
    }
    //public IEnumerable<T1> ToList(Expression<Func<T1, object>> exp)
    //{
    //    SelectHandle(exp.Body);
    //    return InternalQuery<T1>();
    //}
    public DataTable ToDataTable()
    {
        using var conn = dbConnect.CreateConnection();
        var args = BuildArgs();
        Life.BeforeExecute?.Invoke(args);
        var ret = conn.ExecuteTable(args.Sql!, args.SqlParameter);
        args.Done = true;
        Life.AfterExecute?.Invoke(args);
        return ret;
    }
    public Task<DataTable> ToDataTableAsync()
    {
        using var conn = dbConnect.CreateConnection();
        var args = BuildArgs();
        Life.BeforeExecute?.Invoke(args);
        var ret = conn.ExecuteTableAsync(args.Sql!, args.SqlParameter);
        args.Done = true;
        Life.AfterExecute?.Invoke(args);
        return ret;
    }

}
