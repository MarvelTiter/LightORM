#if NET40
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.Repository
{
    public partial interface IRepository<T>
    {
        Task<T?> InsertAsync(T item);
        Task<int> UpdateAsync(T item, Expression<Func<T, bool>>? whereExpression);
        Task<int> UpdateAsync(Expression<Func<object>> updateExpression, Expression<Func<T, bool>>? whereExpression);
        Task<int> DeleteAsync(Expression<Func<T, bool>>? whereExpression);
        Task<T?> GetSingleAsync(Expression<Func<T, bool>>? whereExpression);
        Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? whereExpression, out long total, int index = 0, int size = 0, Expression<Func<T, object>>? orderByExpression = null, bool asc = true);
        Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? whereExpression, Expression<Func<T, object>>? orderByExpression = null, bool asc = true);
    }
}
#endif
