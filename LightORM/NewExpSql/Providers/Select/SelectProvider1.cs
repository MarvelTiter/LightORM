using MDbContext;
using MDbContext.NewExpSql.Interface;
using MDbContext.NewExpSql.Interface.Select;
using MDbContext.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.Providers.Select
{
    internal partial class SelectProvider1<T1> : BasicSelect0<IExpSelect<T1>, T1>, IExpSelect<T1>
    {
        public SelectProvider1(string key, Func<string, ITableContext> getContext, DbConnectInfo connectInfos)
      : base(key, getContext, connectInfos) { }


        public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, TReturn>> exp)
        {
            SelectHandle(exp.Body);
            var conn = dbConnect.CreateConnection();
            return conn.Query<TReturn>(ToSql(), context.GetParameters());
        }

        public IExpSelect<T1> Where(Expression<Func<T1, bool>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }
    }
}
