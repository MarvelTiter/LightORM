using MDbContext.NewExpSql.Interface;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.Providers
{
    internal partial class DeleteProvider<T> :BasicProvider<T>, IExpDelete<T>
    {
        public DeleteProvider(string key, Func<string, ITableContext> getContext, DbConnectInfo connectInfos)
      : base(key, getContext, connectInfos) { }
        public int Execute()
        {
            throw new NotImplementedException();
        }

        public string ToSql()
        {
            throw new NotImplementedException();
        }

        public IExpDelete<T> Where(T item)
        {
            throw new NotImplementedException();
        }

        public IExpDelete<T> Where(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }

        public IExpDelete<T> Where(Expression<Func<T, bool>> exp)
        {
            throw new NotImplementedException();
        }

        public IExpDelete<T> WhereIf(bool condition, Expression<Func<T, bool>> exp)
        {
            throw new NotImplementedException();
        }
    }
}
