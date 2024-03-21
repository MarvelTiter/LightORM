using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Repository
{
    internal class RepositoryImpl<T> : IRepository<T>
    {
        private readonly IExpressionContext context;

        public RepositoryImpl(IExpressionContext context)
        {
            this.context = context;
        }

        public int Count(Expression<Func<T, bool>>? whereExpression)
        {
            Expression<Func<T, bool>> whereExp = whereExpression ?? (e => 1 == 1);
            return context.Select<T>().Where(whereExp).Count();
        }

        public Task<int> CountAsync(Expression<Func<T, bool>>? whereExpression)
        {
            Expression<Func<T, bool>> whereExp = whereExpression ?? (e => 1 == 1);
            return context.Select<T>().Where(whereExp).CountAsync();
        }

        public int Delete(Expression<Func<T, bool>>? whereExpression)
        {
            if (whereExpression == null) return 0;
            return context.Delete<T>().Where(whereExpression).Execute();
        }

        public Task<int> DeleteAsync(Expression<Func<T, bool>>? whereExpression)
        {
            if (whereExpression == null) return Task.FromResult(0);
            return context.Delete<T>().Where(whereExpression).ExecuteAsync();
        }


        public IEnumerable<T> GetList(Expression<Func<T, bool>>? whereExpression, out long total, int index = 0, int size = 0, Expression<Func<T, object>>? orderByExpression = null, bool asc = true)
        {
            Expression<Func<T, bool>> whereExp = whereExpression ?? (e => 1 == 1);
            if (orderByExpression == null)
                return context.Select<T>().Where(whereExp).Count(out total).Paging(index, size).ToList();
            else
                return context.Select<T>().Where(whereExp).Count(out total).OrderBy(orderByExpression, asc).Paging(index, size).ToList();

        }

        public IEnumerable<T> GetList(Expression<Func<T, bool>>? whereExpression, Expression<Func<T, object>>? orderByExpression = null, bool asc = true)
        {
            Expression<Func<T, bool>> whereExp = whereExpression ?? (e => 1 == 1);
            if (orderByExpression == null)
                return context.Select<T>().Where(whereExp).ToList();
            else
                return context.Select<T>().Where(whereExp).OrderBy(orderByExpression, asc).ToList();
        }

        public Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? whereExpression, out long total, int index = 0, int size = 0, Expression<Func<T, object>>? orderByExpression = null, bool asc = true)
        {
            Expression<Func<T, bool>> whereExp = whereExpression ?? (e => 1 == 1);
            if (orderByExpression == null)
                return context.Select<T>().Where(whereExp).Count(out total).Paging(index, size).ToListAsync();
            else
                return context.Select<T>().Where(whereExp).Count(out total).OrderBy(orderByExpression, asc).Paging(index, size).ToListAsync();
        }

        public Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? whereExpression, Expression<Func<T, object>>? orderByExpression = null, bool asc = true)
        {
            Expression<Func<T, bool>> whereExp = whereExpression ?? (e => 1 == 1);
            if (orderByExpression == null)
                return context.Select<T>().Where(whereExp).ToListAsync();
            else
                return context.Select<T>().Where(whereExp).OrderBy(orderByExpression, asc).ToListAsync();
        }

        public T? GetSingle(Expression<Func<T, bool>>? whereExpression)
        {
            Expression<Func<T, bool>> whereExp = whereExpression ?? (e => 1 == 1);
            return context.Select<T>().Where(whereExp).ToList().FirstOrDefault();
        }

        public async Task<T?> GetSingleAsync(Expression<Func<T, bool>>? whereExpression)
        {
            Expression<Func<T, bool>> whereExp = whereExpression ?? (e => 1 == 1);
            var list = await context.Select<T>().Where(whereExp).ToListAsync();
            return list.FirstOrDefault();
        }

        public int Insert(T item)
        {
            return context.Insert(item).Execute();
        }

        public Task<int> InsertAsync(T item)
        {
            return context.Insert(item).ExecuteAsync();
        }

        public TMember Max<TMember>(Expression<Func<T, TMember>> maxExpression, Expression<Func<T, bool>>? whereExpression)
        {
            Expression<Func<T, bool>> whereExp = whereExpression ?? (e => 1 == 1);
            return context.Select<T>().Where(whereExp).Max(maxExpression);
        }

        public Task<TMember> MaxAsync<TMember>(Expression<Func<T, TMember>> maxExpression, Expression<Func<T, bool>>? whereExpression)
        {
            Expression<Func<T, bool>> whereExp = whereExpression ?? (e => 1 == 1);
            return context.Select<T>().Where(whereExp).MaxAsync(maxExpression);
        }

        public int Update(T item, Expression<Func<T, bool>>? whereExpression)
        {
            if (whereExpression == null) return 0;
            return context.Update(item).Where(whereExpression).Execute();
        }

        public int Update(Expression<Func<object>> updateExpression, Expression<Func<T, bool>>? whereExpression)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateAsync(T item, Expression<Func<T, bool>>? whereExpression)
        {
            if (whereExpression == null) return Task.FromResult(0);
            return context.Update(item).Where(whereExpression).ExecuteAsync();
        }

        public Task<int> UpdateAsync(Expression<Func<object>> updateExpression, Expression<Func<T, bool>>? whereExpression)
        {
            throw new NotImplementedException();
        }

    }
}
