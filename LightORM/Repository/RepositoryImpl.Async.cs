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
    internal partial class RepositoryImpl<T> : IRepository<T>
    {
        public Task<int> DeleteAsync(Expression<Func<T, bool>>? whereExpression)
        {
            if (whereExpression == null) throw new ArgumentNullException(nameof(whereExpression));
            return context.Delete<T>().Where(whereExpression).ExecuteAsync();
        }
        public Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? whereExpression, out long total, int index = 0, int size = 0, Expression<Func<T, object>>? orderByExpression = null, bool asc = true)
        {
            var select = context.Select<T>();
            if (whereExpression != null)
                select = select.Where(whereExpression);
            if (index * size > 0)
                select = select.Paging(index, size);
            if (orderByExpression != null)
                select = select.OrderBy(orderByExpression, asc);
            return select.Count(out total).ToListAsync<T>();
        }

        public Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? whereExpression, Expression<Func<T, object>>? orderByExpression = null, bool asc = true)
        {
            var select = context.Select<T>();
            if (whereExpression != null)
                select = select.Where(whereExpression);
            if (orderByExpression != null)
                select = select.OrderBy(orderByExpression, asc);
            return select.ToListAsync<T>();
        }

        public async Task<T?> GetSingleAsync(Expression<Func<T, bool>>? whereExpression)
        {
            if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
            var list = await context.Select<T>().Where(whereExpression).ToListAsync<T>();
            return list.FirstOrDefault();
        }
        public async Task<T?> InsertAsync(T item)
        {
            var flag = await context.Insert<T>().AppendData(item).ExecuteAsync();
            return flag > 0 ? item : default;
        }
        public Task<int> UpdateAsync(T item, Expression<Func<T, bool>>? whereExpression)
        {
            if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
            return context.Update<T>().AppendData(item).Where(whereExpression).ExecuteAsync();
        }

        public Task<int> UpdateAsync(Expression<Func<object>> updateExpression, Expression<Func<T, bool>>? whereExpression)
        {
            if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
            return context.Update<T>().UpdateColumns(updateExpression).Where(whereExpression).ExecuteAsync();
        }

        public Task<int> CountAsync(Expression<Func<T, bool>>? whereExpression)
        {
            if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
            return context.Select<T>().Where(whereExpression).CountAsync();
        }

        public Task<TMember> MaxAsync<TMember>(Expression<Func<T,TMember>> maxExpression,Expression<Func<T, bool>>? whereExpression)
        {
            if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
            return context.Select<T>().Where(whereExpression).MaxAsync(maxExpression);
        }
    }
}
#endif
