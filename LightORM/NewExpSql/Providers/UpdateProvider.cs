using MDbContext.NewExpSql.ExpressionVisitor;
using MDbContext.NewExpSql.Interface;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.Providers
{
    internal partial class UpdateProvider<T> :BasicProvider<T>, IExpUpdate<T>
    {
        SqlFragment ignore;
        SqlFragment update;
        SqlFragment where;
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
            ignore ??= new SqlFragment();
            context.SetFragment(ignore);
            ExpressionVisit.Visit(columns.Body, SqlConfig.Update, context);
            return this;
        }
        public IExpUpdate<T> Set<TField>(Expression<Func<T, TField>> exp)
        {
            update ??= new SqlFragment();
            context.SetFragment(update);
            ExpressionVisit.Visit(exp.Body, SqlConfig.Update, context);
            return this;
        }

        public IExpUpdate<T> SetIf<TField>(bool condition, Expression<Func<T, TField>> exp)
        {
            if (condition) return Set(exp);
            return this;
        }

        public string ToSql()
        {
            throw new NotImplementedException();
        }

        public IExpUpdate<T> UpdateColumns(Expression<Func<T, object>> columns)
        {
            update ??= new SqlFragment();
            context.SetFragment(update);
            ExpressionVisit.Visit(columns.Body, SqlConfig.Update, context);
            return this;
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
