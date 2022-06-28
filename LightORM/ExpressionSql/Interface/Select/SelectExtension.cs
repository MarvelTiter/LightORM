using MDbContext.ExpressionSql.Providers.Select;

namespace MDbContext.ExpressionSql.Interface.Select
{
    public static class SelectExtension
    {
        public static IExpSelect<T1, T2> Select<T1, T2>(this IExpSql self, string key = "MainDb") where T1 : class, new()
        {
            var ins = self as ExpressionCoreSql;
            return new SelectProvider2<T1, T2>(key, ins!.GetContext, ins!.GetDbInfo(key),ins!.Life);
        }

        public static IExpSelect<T1, T2, T3> Select<T1, T2, T3>(this IExpSql self, string key = "MainDb") where T1 : class, new()
        {
            var ins = self as ExpressionCoreSql;
            return new SelectProvider3<T1, T2, T3>(key, ins!.GetContext, ins!.GetDbInfo(key),ins!.Life);
        }

        public static IExpSelect<T1, T2, T3, T4> Select<T1, T2, T3, T4>(this IExpSql self, string key = "MainDb") where T1 : class, new()
        {
            var ins = self as ExpressionCoreSql;
            return new SelectProvider4<T1, T2, T3, T4>(key, ins!.GetContext, ins!.GetDbInfo(key),ins!.Life);
        }

        public static IExpSelect<T1, T2, T3, T4, T5> Select<T1, T2, T3, T4, T5>(this IExpSql self, string key = "MainDb") where T1 : class, new()
        {
            var ins = self as ExpressionCoreSql;
            return new SelectProvider5<T1, T2, T3, T4, T5>(key, ins!.GetContext, ins!.GetDbInfo(key),ins!.Life);
        }

        public static IExpSelect<T1, T2, T3, T4, T5, T6> Select<T1, T2, T3, T4, T5, T6>(this IExpSql self, string key = "MainDb") where T1 : class, new()
        {
            var ins = self as ExpressionCoreSql;
            return new SelectProvider6<T1, T2, T3, T4, T5, T6>(key, ins!.GetContext, ins!.GetDbInfo(key),ins!.Life);
        }

        public static IExpSelect<T1, T2, T3, T4, T5, T6, T7> Select<T1, T2, T3, T4, T5, T6, T7>(this IExpSql self, string key = "MainDb") where T1 : class, new()
        {
            var ins = self as ExpressionCoreSql;
            return new SelectProvider7<T1, T2, T3, T4, T5, T6, T7>(key, ins!.GetContext, ins!.GetDbInfo(key),ins!.Life);
        }

        public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> Select<T1, T2, T3, T4, T5, T6, T7, T8>(this IExpSql self, string key = "MainDb") where T1 : class, new()
        {
            var ins = self as ExpressionCoreSql;
            return new SelectProvider8<T1, T2, T3, T4, T5, T6, T7, T8>(key, ins!.GetContext, ins!.GetDbInfo(key),ins!.Life);
        }

        public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IExpSql self, string key = "MainDb") where T1 : class, new()
        {
            var ins = self as ExpressionCoreSql;
            return new SelectProvider9<T1, T2, T3, T4, T5, T6, T7, T8, T9>(key, ins!.GetContext, ins!.GetDbInfo(key),ins!.Life);
        }

        public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this IExpSql self, string key = "MainDb") where T1 : class, new()
        {
            var ins = self as ExpressionCoreSql;
            return new SelectProvider10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(key, ins!.GetContext, ins!.GetDbInfo(key),ins!.Life);
        }

        public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this IExpSql self, string key = "MainDb") where T1 : class, new()
        {
            var ins = self as ExpressionCoreSql;
            return new SelectProvider11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(key, ins!.GetContext, ins!.GetDbInfo(key),ins!.Life);
        }

        public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this IExpSql self, string key = "MainDb") where T1 : class, new()
        {
            var ins = self as ExpressionCoreSql;
            return new SelectProvider12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(key, ins!.GetContext, ins!.GetDbInfo(key),ins!.Life);
        }
    }
}
