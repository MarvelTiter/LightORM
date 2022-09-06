using MDbContext.ExpressionSql.Ado;
using MDbContext.ExpressionSql.Interface;
using MDbContext.ExpressionSql.Interface.Select;
using System;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql
{
    public interface IExpressionContext
    {
        IExpressionContext SwitchDatabase(string key);
        IExpSelect<T> Select<T>();
        IExpSelect<T> Select<T>(Expression<Func<T, object>> exp);
        IExpInsert<T> Insert<T>();
        IExpUpdate<T> Update<T>();
        IExpDelete<T> Delete<T>();
        IExpressionContext BeginTransaction();
        bool CommitTransaction();
        Task<bool> CommitTransactionAsync();
        IAdo Ado { get; }
    }

    public static class ExpSqlExtensions
    {
        //public static IExpSelect<T> Select<T,T1>(this IExpSql self)
        //{
        //    return new SelectProvider<T>()
        //}
    }
}
