using MDbContext.ExpressionSql.Ado;
using MDbContext.ExpressionSql.Interface.Select;

namespace MDbContext.ExpressionSql.Interface
{
    public interface IExpSql
    {
        IExpSelect<T> Select<T>(string key = ConstString.Main);
        IExpInsert<T> Insert<T>(string key = ConstString.Main);
        IExpUpdate<T> Update<T>(string key = ConstString.Main);
        IExpDelete<T> Delete<T>(string key = ConstString.Main);
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
