using MDbContext;
using MDbContext.NewExpSql.ExpressionVisitor;
using MDbContext.NewExpSql.Interface;
using MDbContext.NewExpSql.Interface.Select;
using MDbContext.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

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

    internal partial class SelectProvider2<T1, T2> : BasicSelect0<IExpSelect<T1, T2>, T1>, IExpSelect<T1, T2>
    {
        public SelectProvider2(string key, Func<string, ITableContext> getContext, DbConnectInfo connectInfos)
     : base(key, getContext, connectInfos)
        {
            context.AddTable(typeof(T2));
        }

        public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, T2, TReturn>> exp)
        {
            SelectHandle(exp.Body);
            var conn = dbConnect.CreateConnection();
            return conn.Query<TReturn>(ToSql(), context.GetParameters());
        }

        public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2>, TReturn>> exp)
        {
            SelectHandle(exp.Body);
            var conn = dbConnect.CreateConnection();
            return conn.Query<TReturn>(ToSql(), context.GetParameters());
        }

        

        public IExpSelect<T1, T2> Where(Expression<Func<T1, T2, bool>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }

        public IExpSelect<T1, T2> Where(Expression<Func<TypeSet<T1, T2>, bool>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }
    }
    internal partial class SelectProvider3<T1, T2, T3> : BasicSelect0<IExpSelect<T1, T2, T3>, T1>, IExpSelect<T1, T2, T3>
    {
        public SelectProvider3(string key, Func<string, ITableContext> getContext, DbConnectInfo connectInfos)
     : base(key, getContext, connectInfos)
        {
            context.AddTable(typeof(T2));
            context.AddTable(typeof(T3));
        }
        public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, TReturn>> exp)
        {
            SelectHandle(exp.Body);
            var conn = dbConnect.CreateConnection();
            return conn.Query<TReturn>(ToSql(), context.GetParameters());
        }

        public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3>, TReturn>> exp)
        {
            SelectHandle(exp.Body);
            var conn = dbConnect.CreateConnection();
            return conn.Query<TReturn>(ToSql(), context.GetParameters());
        }

        public IExpSelect<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }

        public IExpSelect<T1, T2, T3> Where(Expression<Func<TypeSet<T1, T2, T3>, bool>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }
    }
    internal partial class SelectProvider4<T1, T2, T3, T4> : BasicSelect0<IExpSelect<T1, T2, T3, T4>, T1>, IExpSelect<T1, T2, T3, T4>
    {
        public SelectProvider4(string key, Func<string, ITableContext> getContext, DbConnectInfo connectInfos)
     : base(key, getContext, connectInfos)
        {
            context.AddTable(typeof(T2));
            context.AddTable(typeof(T3));
            context.AddTable(typeof(T4));
        }
        public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, T4, TReturn>> exp)
        {
            SelectHandle(exp.Body);
            var conn = dbConnect.CreateConnection();
            return conn.Query<TReturn>(ToSql(), context.GetParameters());
        }

        public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<T1, T2, T3, T4>, TReturn>> exp)
        {
            SelectHandle(exp.Body);
            var conn = dbConnect.CreateConnection();
            return conn.Query<TReturn>(ToSql(), context.GetParameters());
        }

        public IExpSelect<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }

        public IExpSelect<T1, T2, T3, T4> Where(Expression<Func<TypeSet<T1, T2, T3, T4>, bool>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }
    }
}
