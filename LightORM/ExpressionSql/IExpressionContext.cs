using MDbContext.ExpressionSql.Ado;
using MDbContext.ExpressionSql.Interface;
using MDbContext.ExpressionSql.Interface.Select;
using System.Data;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql
{
    public interface IExpressionContext
    {
        IExpSelect<T> Select<T>(string key = ConstString.Main);
        IExpInsert<T> Insert<T>(string key = ConstString.Main);
        IExpUpdate<T> Update<T>(string key = ConstString.Main);
        IExpDelete<T> Delete<T>(string key = ConstString.Main);
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
