using MDbContext.ExpressionSql.Providers.Select;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Interface.Select
{
    public static class SelectDynamicExtension
    {        
        public static IEnumerable<dynamic> ToDynamicList<T1>(this IExpSelect<T1> self, Expression<Func<T1, object>> exp)
        {
            var ins = self as SelectProvider1<T1>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1>(this IExpSelect<T1> self, Expression<Func<T1, object>> exp)
        {
            var ins = self as SelectProvider1<T1>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        #region T`2
        public static IEnumerable<dynamic> ToDynamicList<T1, T2>(this IExpSelect<T1, T2> self, Expression<Func<T1, T2, object>> exp)
        {
            var ins = self as SelectProvider2<T1, T2>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2>(this IExpSelect<T1, T2> self, Expression<Func<T1, T2, object>> exp)
        {
            var ins = self as SelectProvider2<T1, T2>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        public static IEnumerable<dynamic> ToDynamicList<T1, T2>(this IExpSelect<T1, T2> self, Expression<Func<TypeSet<T1, T2>, object>> exp)
        {
            var ins = self as SelectProvider2<T1, T2>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2>(this IExpSelect<T1, T2> self, Expression<Func<TypeSet<T1, T2>, object>> exp)
        {
            var ins = self as SelectProvider2<T1, T2>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        #endregion

        #region T`3
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3>(this IExpSelect<T1, T2, T3> self, Expression<Func<T1, T2, T3, object>> exp)
        {
            var ins = self as SelectProvider3<T1, T2, T3>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3>(this IExpSelect<T1, T2, T3> self, Expression<Func<T1, T2, T3, object>> exp)
        {
            var ins = self as SelectProvider3<T1, T2, T3>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3>(this IExpSelect<T1, T2, T3> self, Expression<Func<TypeSet<T1, T2, T3>, object>> exp)
        {
            var ins = self as SelectProvider3<T1, T2, T3>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3>(this IExpSelect<T1, T2, T3> self, Expression<Func<TypeSet<T1, T2, T3>, object>> exp)
        {
            var ins = self as SelectProvider3<T1, T2, T3>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        #endregion

        #region T`4
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3, T4>(this IExpSelect<T1, T2, T3, T4> self, Expression<Func<T1, T2, T3, T4, object>> exp)
        {
            var ins = self as SelectProvider4<T1, T2, T3, T4>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3, T4>(this IExpSelect<T1, T2, T3, T4> self, Expression<Func<T1, T2, T3, T4, object>> exp)
        {
            var ins = self as SelectProvider4<T1, T2, T3, T4>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3, T4>(this IExpSelect<T1, T2, T3, T4> self, Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp)
        {
            var ins = self as SelectProvider4<T1, T2, T3, T4>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3, T4>(this IExpSelect<T1, T2, T3, T4> self, Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp)
        {
            var ins = self as SelectProvider4<T1, T2, T3, T4>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        #endregion

        #region T`5
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3, T4, T5>(this IExpSelect<T1, T2, T3, T4, T5> self, Expression<Func<T1, T2, T3, T4, T5, object>> exp)
        {
            var ins = self as SelectProvider5<T1, T2, T3, T4, T5>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3, T4, T5>(this IExpSelect<T1, T2, T3, T4, T5> self, Expression<Func<T1, T2, T3, T4, T5, object>> exp)
        {
            var ins = self as SelectProvider5<T1, T2, T3, T4, T5>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3, T4, T5>(this IExpSelect<T1, T2, T3, T4, T5> self, Expression<Func<TypeSet<T1, T2, T3, T4, T5>, object>> exp)
        {
            var ins = self as SelectProvider5<T1, T2, T3, T4, T5>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3, T4, T5>(this IExpSelect<T1, T2, T3, T4, T5> self, Expression<Func<TypeSet<T1, T2, T3, T4, T5>, object>> exp)
        {
            var ins = self as SelectProvider5<T1, T2, T3, T4, T5>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        #endregion

        #region T`6
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3, T4, T5, T6>(this IExpSelect<T1, T2, T3, T4, T5, T6> self, Expression<Func<T1, T2, T3, T4, T5, T6, object>> exp)
        {
            var ins = self as SelectProvider6<T1, T2, T3, T4, T5, T6>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3, T4, T5, T6>(this IExpSelect<T1, T2, T3, T4, T5, T6> self, Expression<Func<T1, T2, T3, T4, T5, T6, object>> exp)
        {
            var ins = self as SelectProvider6<T1, T2, T3, T4, T5, T6>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3, T4, T5, T6>(this IExpSelect<T1, T2, T3, T4, T5, T6> self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, object>> exp)
        {
            var ins = self as SelectProvider6<T1, T2, T3, T4, T5, T6>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3, T4, T5, T6>(this IExpSelect<T1, T2, T3, T4, T5, T6> self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, object>> exp)
        {
            var ins = self as SelectProvider6<T1, T2, T3, T4, T5, T6>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        #endregion

        #region T`7
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3, T4, T5, T6, T7>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7> self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, object>> exp)
        {
            var ins = self as SelectProvider7<T1, T2, T3, T4, T5, T6, T7>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3, T4, T5, T6, T7>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7> self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, object>> exp)
        {
            var ins = self as SelectProvider7<T1, T2, T3, T4, T5, T6, T7>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3, T4, T5, T6, T7>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7> self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, object>> exp)
        {
            var ins = self as SelectProvider7<T1, T2, T3, T4, T5, T6, T7>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3, T4, T5, T6, T7>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7> self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, object>> exp)
        {
            var ins = self as SelectProvider7<T1, T2, T3, T4, T5, T6, T7>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        #endregion

        #region T`8
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3, T4, T5, T6, T7, T8>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, object>> exp)
        {
            var ins = self as SelectProvider8<T1, T2, T3, T4, T5, T6, T7, T8>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3, T4, T5, T6, T7, T8>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, object>> exp)
        {
            var ins = self as SelectProvider8<T1, T2, T3, T4, T5, T6, T7, T8>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3, T4, T5, T6, T7, T8>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, object>> exp)
        {
            var ins = self as SelectProvider8<T1, T2, T3, T4, T5, T6, T7, T8>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3, T4, T5, T6, T7, T8>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, object>> exp)
        {
            var ins = self as SelectProvider8<T1, T2, T3, T4, T5, T6, T7, T8>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        #endregion

        #region T`9
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, object>> exp)
        {
            var ins = self as SelectProvider9<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, object>> exp)
        {
            var ins = self as SelectProvider9<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, object>> exp)
        {
            var ins = self as SelectProvider9<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, object>> exp)
        {
            var ins = self as SelectProvider9<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        #endregion

        #region T`10
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> exp)
        {
            var ins = self as SelectProvider10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> exp)
        {
            var ins = self as SelectProvider10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, object>> exp)
        {
            var ins = self as SelectProvider10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, object>> exp)
        {
            var ins = self as SelectProvider10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        #endregion

        #region T`11
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, object>> exp)
        {
            var ins = self as SelectProvider11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, object>> exp)
        {
            var ins = self as SelectProvider11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, object>> exp)
        {
            var ins = self as SelectProvider11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, object>> exp)
        {
            var ins = self as SelectProvider11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        #endregion

        #region T`12
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, object>> exp)
        {
            var ins = self as SelectProvider12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, object>> exp)
        {
            var ins = self as SelectProvider12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        public static IEnumerable<dynamic> ToDynamicList<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, object>> exp)
        {
            var ins = self as SelectProvider12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQuery();
        }
        public static Task<IList<dynamic>> ToDynamicListAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, object>> exp)
        {
            var ins = self as SelectProvider12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
            ins!.SelectHandle(exp.Body);
            return ins!.InternalQueryAsync();
        }
        #endregion
    }
}
