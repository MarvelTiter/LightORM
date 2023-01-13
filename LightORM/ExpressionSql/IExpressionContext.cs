using MDbContext.ExpressionSql.Ado;
using MDbContext.ExpressionSql.Interface;
using MDbContext.ExpressionSql.Interface.Select;
using System;
using System.Data;
using System.Linq.Expressions;
#if NET40
#else
using System.Threading.Tasks;
#endif
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
#if NET40
#else
        Task<bool> CommitTransactionAsync();
#endif
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
