using MDbContext.NewExpSql.Interface;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.Providers
{
    internal partial class UpdateProvider<T> :BasicProvider<T>, IExpUpdate<T>
    {
        public UpdateProvider(string key, Func<string, ITableContext> getContext, DbConnectInfo connectInfos)
      : base(key, getContext, connectInfos) { }
        public IExpUpdate<T> AppendData(T item)
        {
            throw new NotImplementedException();
        }

        public int Execute()
        {
            throw new NotImplementedException();
        }

        public IExpUpdate<T> IgnoreColumns(Expression<Func<T, object>> columns)
        {
            throw new NotImplementedException();
        }

        public IExpUpdate<T> Set<TField>(Expression<Func<T, TField>> exp)
        {
            throw new NotImplementedException();
        }

        public IExpUpdate<T> SetIf<TField>(bool confition, Expression<Func<T, TField>> exp)
        {
            throw new NotImplementedException();
        }

        public string ToSql()
        {
            throw new NotImplementedException();
        }

        public IExpInsert<T> UpdateColumns(Expression<Func<T, object>> columns)
        {
            throw new NotImplementedException();
        }

        public IExpUpdate<T> Where(T item)
        {
            throw new NotImplementedException();
        }

        public IExpUpdate<T> Where(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }

        public IExpUpdate<T> Where(Expression<Func<T, bool>> exp)
        {
            throw new NotImplementedException();
        }

        public IExpUpdate<T> WhereIf(bool condition, Expression<Func<T, bool>> exp)
        {
            throw new NotImplementedException();
        }
    }
}
