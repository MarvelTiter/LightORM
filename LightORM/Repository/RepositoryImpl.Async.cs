#if NET40
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MDbContext.Repository
{
    internal partial class RepositoryImpl<T> : IRepository<T>
    {
		public Task<int> DeleteAsync(Expression<Func<T, bool>>? whereExpression) => DeleteAsync(whereExpression, CancellationToken.None);
		public Task<int> DeleteAsync(Expression<Func<T, bool>>? whereExpression, CancellationToken token)
		{
			if (whereExpression == null) throw new ArgumentNullException(nameof(whereExpression));
			return context.Delete<T>().Where(whereExpression).AttachCancellationToken(token).ExecuteAsync();
		}

		public Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? whereExpression, out long total, int index = 0, int size = 0, Expression<Func<T, object>>? orderByExpression = null, bool asc = true) => GetListAsync(whereExpression, out total, CancellationToken.None, index, size, orderByExpression, asc);
		public Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? whereExpression, out long total, CancellationToken token, int index = 0, int size = 0, Expression<Func<T, object>>? orderByExpression = null, bool asc = true)
		{
			var select = context.Select<T>();
			if (whereExpression != null)
				select = select.Where(whereExpression);
			if (index * size > 0)
				select = select.Paging(index, size);
			if (orderByExpression != null)
				select = select.OrderBy(orderByExpression, asc);
			return select.AttachCancellationToken(token).Count(out total).ToListAsync<T>();
		}

		public Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? whereExpression, Expression<Func<T, object>>? orderByExpression = null, bool asc = true) => GetListAsync(whereExpression, CancellationToken.None, orderByExpression, asc);
		public Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? whereExpression, CancellationToken token, Expression<Func<T, object>>? orderByExpression = null, bool asc = true)
		{
			var select = context.Select<T>();
			if (whereExpression != null)
				select = select.Where(whereExpression);
			if (orderByExpression != null)
				select = select.OrderBy(orderByExpression, asc);
			return select.AttachCancellationToken(token).ToListAsync<T>();
		}

		public Task<T?> GetSingleAsync(Expression<Func<T, bool>>? whereExpression) => GetSingleAsync(whereExpression, CancellationToken.None);
		public async Task<T?> GetSingleAsync(Expression<Func<T, bool>>? whereExpression, CancellationToken token)
		{
			if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
			var list = await context.Select<T>().Where(whereExpression).AttachCancellationToken(token).ToListAsync();
			return list.FirstOrDefault();
		}

		public Task<T?> InsertAsync(T item) => InsertAsync(item, CancellationToken.None);
		public async Task<T?> InsertAsync(T item, CancellationToken token)
		{
			var flag = await context.Insert<T>().AppendData(item).AttachCancellationToken(token).ExecuteAsync();
			return flag > 0 ? item : default;
		}
        public Task<int> UpdateAsync(T item, Expression<Func<T, bool>>? whereExpression) => UpdateAsync(item, whereExpression, CancellationToken.None);
		public Task<int> UpdateAsync(T item, Expression<Func<T, bool>>? whereExpression, CancellationToken token)
		{
			if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
			return context.Update<T>().AppendData(item).Where(whereExpression).AttachCancellationToken(token).ExecuteAsync();
		}

        public Task<int> UpdateAsync(Expression<Func<object>> updateExpression, Expression<Func<T, bool>>? whereExpression) => UpdateAsync(updateExpression, whereExpression, CancellationToken.None);
		public Task<int> UpdateAsync(Expression<Func<object>> updateExpression, Expression<Func<T, bool>>? whereExpression, CancellationToken token)
		{
			if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
			return context.Update<T>().UpdateColumns(updateExpression).Where(whereExpression).AttachCancellationToken(token).ExecuteAsync();
		}

		public Task<int> CountAsync(Expression<Func<T, bool>>? whereExpression) => CountAsync(whereExpression, CancellationToken.None);
		public Task<int> CountAsync(Expression<Func<T, bool>>? whereExpression, CancellationToken token)
		{
			if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
			return context.Select<T>().Where(whereExpression).AttachCancellationToken(token).CountAsync();
		}

		public Task<TMember> MaxAsync<TMember>(Expression<Func<T, TMember>> maxExpression, Expression<Func<T, bool>>? whereExpression) => MaxAsync<TMember>(maxExpression, whereExpression, CancellationToken.None);
		public Task<TMember> MaxAsync<TMember>(Expression<Func<T, TMember>> maxExpression, Expression<Func<T, bool>>? whereExpression, CancellationToken token)
		{
			if (whereExpression is null) throw new ArgumentNullException(nameof(whereExpression));
			return context.Select<T>().Where(whereExpression).AttachCancellationToken(token).MaxAsync(maxExpression);
		}		
	}
}
#endif
