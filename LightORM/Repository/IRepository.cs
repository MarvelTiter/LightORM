﻿using MDbContext.ExpressionSql;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.Repository;

public interface IRepository<T>
{
    IRepository<T> SwitchDatabase(string key);
    T? Insert(T item);
    int Update(T item, Expression<Func<T, bool>>? whereExpression);
    int Update(Expression<Func<object>> updateExpression, Expression<Func<T, bool>>? whereExpression);
    int Delete(Expression<Func<T, bool>>? whereExpression);
    T? GetSingle(Expression<Func<T, bool>>? whereExpression);
    IEnumerable<T> GetList(Expression<Func<T, bool>>? whereExpression, out long total, int index = 0, int size = 0, Expression<Func<T, object>>? orderByExpression = null, bool asc = true);
    IEnumerable<T> GetList(Expression<Func<T, bool>>? whereExpression, Expression<Func<T, object>>? orderByExpression = null, bool asc = true);

    #region Async 
    Task<T?> InsertAsync(T item);
    Task<int> UpdateAsync(T item, Expression<Func<T, bool>>? whereExpression);
    Task<int> UpdateAsync(Expression<Func<object>> updateExpression, Expression<Func<T, bool>>? whereExpression);
    Task<int> DeleteAsync(Expression<Func<T, bool>>? whereExpression);
    Task<T?> GetSingleAsync(Expression<Func<T, bool>>? whereExpression);
    Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? whereExpression, out long total, int index = 0, int size = 0, Expression<Func<T, object>>? orderByExpression = null, bool asc = true);
    Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? whereExpression, Expression<Func<T, object>>? orderByExpression = null, bool asc = true);
    #endregion
}
