using MDbContext.NewExpSql.Interface;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.Providers
{
    internal partial class InsertProvider<T> : BasicProvider<T>, IExpInsert<T>
    {
        public InsertProvider(string key, Func<string, (ITableContext context, DbConnectInfo info)> getDbInfos)
        : base(key, getDbInfos) { }
        public IExpInsert<T> AppendData(T item)
        {
            throw new NotImplementedException();
        }

        public IExpInsert<T> AppendData(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }

        public int Execute()
        {
            throw new NotImplementedException();
        }

        public IExpInsert<T> IgnoreColumns(Expression<Func<T, object>> columns)
        {
            throw new NotImplementedException();
        }

        public IExpInsert<T> SetColumns(Expression<Func<T, object>> columns)
        {
            throw new NotImplementedException();
        }

        public string ToSql()
        {
            throw new NotImplementedException();
        }

        public IExpInsert<T> Where(Expression<Func<T, bool>> exp)
        {
            throw new NotImplementedException();
        }

        public IExpInsert<T> WhereIf(bool condition, Expression<Func<T, bool>> exp)
        {
            throw new NotImplementedException();
        }
    }
}
