using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql.Providers
{
    internal class SelectProvider<T> : IExpSelect<T>
    {
        private readonly ITableContext tableContext;
        private readonly DbConnectInfo dbConnect;
        public SelectProvider(string key, Func<string, (ITableContext context, DbConnectInfo info)> getDbInfos)
        {
            var result = getDbInfos.Invoke(key);
            tableContext = result.context;
            dbConnect = result.info;
        }
        public IExpSelect<T> Count(out long total)
        {
            throw new NotImplementedException();
        }

        public int Execute()
        {
            throw new NotImplementedException();
        }

        public IExpSelect<T> InnerJoin<T1>(Expression<Func<T, T1, object>> exp)
        {
            throw new NotImplementedException();
        }

        public IExpSelect<T> LeftJoin<T1>(Expression<Func<T, T1, object>> exp)
        {
            throw new NotImplementedException();
        }

        public IExpSelect<T> RightJoin<T1>(Expression<Func<T, T1, object>> exp)
        {
            throw new NotImplementedException();
        }

        public string ToSql()
        {
            throw new NotImplementedException();
        }

        public IExpSelect<T> Where<T1>(Expression<Func<T1, bool>> exp)
        {
            throw new NotImplementedException();
        }

        public IExpSelect<T> Where<T1>(Expression<Func<T, T1, bool>> exp)
        {
            throw new NotImplementedException();
        }

        public IExpSelect<T> Where(Expression<Func<T, bool>> exp)
        {
            throw new NotImplementedException();
        }

        public IExpSelect<T> WhereIf(bool condition, Expression<Func<T, bool>> exp)
        {
            throw new NotImplementedException();
        }
    }
}
