using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql
{
    public interface ISql<TPart, T>
    {
        TPart Where(Expression<Func<T, bool>> exp);
        TPart WhereIf(bool condition, Expression<Func<T, bool>> exp);
        int Execute();
        string ToSql();
    }

    public interface IExpSelect<T> : ISql<IExpSelect<T>, T>
    {
        IExpSelect<T> InnerJoin<T1>(Expression<Func<T, T1, object>> exp);
        IExpSelect<T> LeftJoin<T1>(Expression<Func<T, T1, object>> exp);
        IExpSelect<T> RightJoin<T1>(Expression<Func<T, T1, object>> exp);
        //IExpSelect<T> InnerJoin<T1, T2>(Expression<Func<T, T1, T2, object>> exp);
        //IExpSelect<T> InnerJoin<T1, T2>(Expression<Func<T1, T2, object>> exp);
        //IExpSelect<T> LeftJoin<T1, T2>(Expression<Func<T, T1, T2, object>> exp);
        //IExpSelect<T> LeftJoin<T1, T2>(Expression<Func<T1, T2, object>> exp);
        //IExpSelect<T> RightJoin<T1, T2>(Expression<Func<T, T1, T2, object>> exp);
        //IExpSelect<T> RightJoin<T1, T2>(Expression<Func<T1, T2, object>> exp);
        IExpSelect<T> Where<T1>(Expression<Func<T1, bool>> exp);
        IExpSelect<T> Where<T1>(Expression<Func<T, T1, bool>> exp);
        //IExpSelect<T> Where<T1, T2>(Expression<Func<T, T1, T2, bool>> exp);
        //IExpSelect<T> Where<T1, T2>(Expression<Func<T1, T2, bool>> exp);
        //IExpSelect<T> Where<T1, T2, T3>(Expression<Func<T, T1, T2, T3, bool>> exp);
        //IExpSelect<T> Where<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> exp);
        IExpSelect<T> Count(out long total);
    }
    public interface IExpInsert<T> : ISql<IExpInsert<T>, T>
    {
        IExpInsert<T> AppendData(T item);
        IExpInsert<T> AppendData(IEnumerable<T> items);
        IExpInsert<T> SetColumns(Expression<Func<T, object>> columns);
        IExpInsert<T> IgnoreColumns(Expression<Func<T, object>> columns);
    }
    public interface IExpUpdate<T> : ISql<IExpUpdate<T>, T>
    {
        IExpUpdate<T> AppendData(T item);
        IExpInsert<T> UpdateColumns(Expression<Func<T, object>> columns);
        IExpUpdate<T> IgnoreColumns(Expression<Func<T, object>> columns);
        IExpUpdate<T> Set<TField>(Expression<Func<T, TField>> exp);
        IExpUpdate<T> SetIf<TField>(bool confition, Expression<Func<T, TField>> exp);
        IExpUpdate<T> Where(T item);
        IExpUpdate<T> Where(IEnumerable<T> items);
    }
    public interface IExpDelete<T> : ISql<IExpDelete<T>, T>
    {
        IExpDelete<T> Where(T item);
        IExpDelete<T> Where(IEnumerable<T> items);
    }
    public interface IExpSql
    {
        IExpSelect<T> Select<T>();
        IExpInsert<T> Insert<T>();
        IExpUpdate<T> Update<T>();
        IExpDelete<T> Delete<T>();
    }
    internal struct DbConnectInfo
    {
        public Func<IDbConnection> CreateConnection;
        public DbBaseType DbBaseType;
    }
}
