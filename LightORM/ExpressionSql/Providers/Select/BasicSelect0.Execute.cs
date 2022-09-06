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
        return InternalQueryAsync<T1>();
    }

    public Task<IList<dynamic>> ToListAsync()
    {
        SelectHandle(selectBody);
        return InternalQueryAsync();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>()
    {
        //Expression<Func<TReturn, object>> exp = r => new { r };
        //SelectHandle(exp.Body);
        SelectHandle(selectBody);
        return InternalQueryAsync<TReturn>();
    }

    public IEnumerable<dynamic> ToList()
    {
        SelectHandle(selectBody);
        return InternalQuery();
    }
    public IEnumerable<TReturn> ToList<TReturn>()
    {
        //Expression<Func<TReturn, object>> exp = r => new { r };
        SelectHandle(selectBody);
        return InternalQuery<TReturn>();
    }
    //public IEnumerable<T1> ToList(Expression<Func<T1, object>> exp)
    //{
    //    SelectHandle(exp.Body);
    //    return InternalQuery<T1>();
    //}

    public DataTable ToDataTable()
    {
        throw new NotImplementedException();
    }

    public Task<DataTable> ToDataTableAsync()
    {
        throw new NotImplementedException();
    }

    internal TReturn InternalExecute<TReturn>()
    {
        var sql = ToSql();
        var param = context.GetParameters();
        using var conn = dbConnect.CreateConnection();
        return conn.QuerySingle<TReturn>(sql, param);
    }

    internal Task<TReturn> InternalExecuteAsync<TReturn>()
    {
        var sql = ToSql();
        var param = context.GetParameters();
        using var conn = dbConnect.CreateConnection();
        return conn.QuerySingleAsync<TReturn>(sql, param);
    }

    internal TReturn InternalExecute<TReturn>(string sql, object param)
    {
        using var conn = dbConnect.CreateConnection();
        return conn.QuerySingle<TReturn>(sql, param);
    }

    internal IEnumerable<TReturn> InternalQuery<TReturn>()
    {
        var sql = ToSql();
        var param = context.GetParameters();
        var conn = dbConnect.CreateConnection();
        return conn.Query<TReturn>(sql, param);
    }

    internal IEnumerable<dynamic> InternalQuery()
    {
        var sql = ToSql();
        var param = context.GetParameters();
        var conn = dbConnect.CreateConnection();
        return conn.Query(sql, param);
    }

    internal async Task<IList<TReturn>> InternalQueryAsync<TReturn>()
    {
        var sql = ToSql();
        var param = context.GetParameters();
        using var conn = dbConnect.CreateConnection();
        var list = await conn.QueryAsync<TReturn>(sql, param);
        return list;
    }
    internal async Task<IList<dynamic>> InternalQueryAsync()
    {
        var sql = ToSql();
        var param = context.GetParameters();
        using var conn = dbConnect.CreateConnection();
        var list = await conn.QueryAsync(sql, param);
        return list.ToList();
    }

    
}
