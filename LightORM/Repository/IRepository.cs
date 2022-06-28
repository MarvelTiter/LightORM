using MDbContext.ExpressionSql;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.Repository;

public interface IRepository<T>
{
    T? Insert(T item, string key = ConstString.Main);
    int Update(T item, Expression<Func<T, bool>>? whereExpression, string key = ConstString.Main);
    int Update(Expression<Func<object>> updateExpression, Expression<Func<T, bool>>? whereExpression, string key = ConstString.Main);
    int Delete(Expression<Func<T, bool>>? whereExpression, string key = ConstString.Main);
    T? GetSingle(Expression<Func<T, bool>>? whereExpression, string key = ConstString.Main);
    IEnumerable<T> GetList(Expression<Func<T, bool>>? whereExpression, out long total, int index = 0, int size = 0, Expression<Func<T, object>>? orderByExpression = null, bool asc = true, string key = ConstString.Main);
    IEnumerable<T> GetList(Expression<Func<T, bool>>? whereExpression, Expression<Func<T, object>>? orderByExpression = null, bool asc = true, string key = ConstString.Main);

    #region Async 
    Task<T?> InsertAsync(T item, string key = ConstString.Main);
    Task<int> UpdateAsync(T item, Expression<Func<T, bool>>? whereExpression, string key = ConstString.Main);
    Task<int> UpdateAsync(Expression<Func<object>> updateExpression, Expression<Func<T, bool>>? whereExpression, string key = ConstString.Main);
    Task<int> DeleteAsync(Expression<Func<T, bool>>? whereExpression, string key = ConstString.Main);
    Task<T?> GetSingleAsync(Expression<Func<T, bool>>? whereExpression, string key = ConstString.Main);
    Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? whereExpression, out long total, int index = 0, int size = 0, Expression<Func<T, object>>? orderByExpression = null, bool asc = true, string key = ConstString.Main);
    Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? whereExpression, Expression<Func<T, object>>? orderByExpression = null, bool asc = true, string key = ConstString.Main);
    #endregion
}
