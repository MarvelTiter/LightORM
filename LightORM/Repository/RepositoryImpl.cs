using MDbContext.ExpressionSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MDbContext.Repository;

public class RepositoryImpl<T> : IRepository<T>
{
    private readonly IExpressionContext context;

    public RepositoryImpl(IExpressionContext context)
    {
        this.context = context;
    }

    public int Delete(Expression<Func<T, bool>>? whereExpression, string key = "MainDb")
    {
        if (whereExpression == null) throw new ArgumentNullException(nameof(whereExpression));
        return context.Delete<T>(key).Where(whereExpression).Execute();
    }

    public Task<int> DeleteAsync(Expression<Func<T, bool>>? whereExpression, string key = "MainDb")
    {
        if (whereExpression == null) throw new ArgumentNullException(nameof(whereExpression));
        return context.Delete<T>(key).Where(whereExpression).ExecuteAsync();
    }

    public IEnumerable<T> GetList(Expression<Func<T, bool>>? whereExpression, out long total, int index = 0, int size = 0, Expression<Func<T, object>>? orderByExpression = null, bool asc = true, string key = "MainDb")
    {
        var select = context.Select<T>(key);
        if (whereExpression != null)
            select = select.Where(whereExpression);
        if (index * size > 0)
            select = select.Paging(index, size);
        if (orderByExpression != null)
            select = select.OrderBy(orderByExpression, asc);
        return select.Count(out total).ToList();
    }

    public IEnumerable<T> GetList(Expression<Func<T, bool>>? whereExpression, Expression<Func<T, object>>? orderByExpression = null, bool asc = true, string key = "MainDb")
    {
        var select = context.Select<T>(key);
        if (whereExpression != null)
            select = select.Where(whereExpression);
        if (orderByExpression != null)
            select = select.OrderBy(orderByExpression, asc);
        return select.ToList();
    }

    public Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? whereExpression, out long total, int index = 0, int size = 0, Expression<Func<T, object>>? orderByExpression = null, bool asc = true, string key = "MainDb")
    {
        var select = context.Select<T>(key);
        if (whereExpression != null)
            select = select.Where(whereExpression);
        if (index * size > 0)
            select = select.Paging(index, size);
        if (orderByExpression != null)
            select = select.OrderBy(orderByExpression, asc);
        return select.Count(out total).ToListAsync();
    }

    public Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? whereExpression, Expression<Func<T, object>>? orderByExpression = null, bool asc = true, string key = "MainDb")
    {
        var select = context.Select<T>(key);
        if (whereExpression != null)
            select = select.Where(whereExpression);
        if (orderByExpression != null)
            select = select.OrderBy(orderByExpression, asc);
        return select.ToListAsync();
    }

    public T? GetSingle(Expression<Func<T, bool>>? whereExpression, string key = "MainDb")
    {
        if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
        return context.Select<T>(key).Where(whereExpression).ToList().FirstOrDefault();
    }

    public async Task<T?> GetSingleAsync(Expression<Func<T, bool>>? whereExpression, string key = "MainDb")
    {
        if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
        var list = await context.Select<T>(key).Where(whereExpression).ToListAsync();
        return list.FirstOrDefault();
    }

    public T? Insert(T item, string key = "MainDb")
    {
        var flag = context.Insert<T>(key).AppendData(item).Execute();
        return flag > 0 ? item : default;
    }

    public async Task<T?> InsertAsync(T item, string key = "MainDb")
    {
        var flag = await context.Insert<T>(key).AppendData(item).ExecuteAsync();
        return flag > 0 ? item : default;
    }

    public int Update(T item, Expression<Func<T, bool>>? whereExpression, string key = "MainDb")
    {
        if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
        return context.Update<T>(key).AppendData(item).Where(whereExpression).Execute();
    }

    public int Update(Expression<Func<object>> updateExpression, Expression<Func<T, bool>>? whereExpression, string key = "MainDb")
    {
        if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
        return context.Update<T>(key).UpdateColumns(updateExpression).Where(whereExpression).Execute();
    }

    public Task<int> UpdateAsync(T item, Expression<Func<T, bool>>? whereExpression, string key = "MainDb")
    {
        if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
        return context.Update<T>(key).AppendData(item).Where(whereExpression).ExecuteAsync();
    }

    public Task<int> UpdateAsync(Expression<Func<object>> updateExpression, Expression<Func<T, bool>>? whereExpression, string key = "MainDb")
    {
        if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
        return context.Update<T>(key).UpdateColumns(updateExpression).Where(whereExpression).ExecuteAsync();
    }
}
