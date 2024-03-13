using LightORM.ExpressionSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MDbContext.Repository;
internal partial class RepositoryImpl<T> : IRepository<T>
{
    private readonly IExpressionContext context;
    private string _dbKey = ConstString.Main;
    public RepositoryImpl(IExpressionContext context)
    {
        this.context = context;
    }

    public IRepository<T> SwitchDatabase(string key)
    {
        _dbKey = key;
        context.SwitchDatabase(key);
        return this;
    }

    public int Delete(Expression<Func<T, bool>>? whereExpression)
    {
        if (whereExpression == null) throw new ArgumentNullException(nameof(whereExpression));
        return context.Delete<T>().Where(whereExpression).Execute();
    }    

    public IEnumerable<T> GetList(Expression<Func<T, bool>>? whereExpression, out long total, int index = 0, int size = 0, Expression<Func<T, object>>? orderByExpression = null, bool asc = true)
    {
        var select = context.Select<T>();
        if (whereExpression != null)
            select = select.Where(whereExpression);
        if (index * size > 0)
            select = select.Paging(index, size);
        if (orderByExpression != null)
            select = select.OrderBy(orderByExpression, asc);
        return select.Count(out total).ToList<T>();
    }

    public IEnumerable<T> GetList(Expression<Func<T, bool>>? whereExpression, Expression<Func<T, object>>? orderByExpression = null, bool asc = true)
    {
        var select = context.Select<T>();
        if (whereExpression != null)
            select = select.Where(whereExpression);
        if (orderByExpression != null)
            select = select.OrderBy(orderByExpression, asc);
        return select.ToList<T>();
    }    

    public T? GetSingle(Expression<Func<T, bool>>? whereExpression)
    {
        if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
        var list = context.Select<T>().Where(whereExpression).ToList<T>();
        return list.FirstOrDefault();
    }    

    public T? Insert(T item)
    {
        var flag = context.Insert<T>().AppendData(item).Execute();
        return flag > 0 ? item : default;
    }   

    public int Update(T item, Expression<Func<T, bool>>? whereExpression)
    {
        if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
        return context.Update<T>().AppendData(item).Where(whereExpression).Execute();
    }

    public int Update(Expression<Func<object>> updateExpression, Expression<Func<T, bool>>? whereExpression)
    {
        if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
        return context.Update<T>().UpdateColumns(updateExpression).Where(whereExpression).Execute();
    }

    public int Count(Expression<Func<T, bool>>? whereExpression)
    {
        if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
        return context.Select<T>().Where(whereExpression).Count();
    }

    public TMember Max<TMember>(Expression<Func<T, TMember>> maxExpression, Expression<Func<T, bool>>? whereExpression)
    {
        if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
        return context.Select<T>().Where(whereExpression).Max(maxExpression);
    }
}
