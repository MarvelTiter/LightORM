using MDbContext.ExpressionSql.Interface.Select;
using MDbContext.ExpressionSql.Providers.Select;

namespace MDbContext.ExpressionSql;

public static class SelectExtension
{
    public static IExpSelect<T1, T2> Select<T1, T2>(this IExpressionContext self, string key = "MainDb") where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return new SelectProvider2<T1, T2>(key, ins!.GetContext, ins!.GetDbInfo(key), ins!.Life);
    }

    public static IExpSelect<T1, T2, T3> Select<T1, T2, T3>(this IExpressionContext self, string key = "MainDb") where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return new SelectProvider3<T1, T2, T3>(key, ins!.GetContext, ins!.GetDbInfo(key), ins!.Life);
    }

    public static IExpSelect<T1, T2, T3, T4> Select<T1, T2, T3, T4>(this IExpressionContext self, string key = "MainDb") where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return new SelectProvider4<T1, T2, T3, T4>(key, ins!.GetContext, ins!.GetDbInfo(key), ins!.Life);
    }

    public static IExpSelect<T1, T2, T3, T4, T5> Select<T1, T2, T3, T4, T5>(this IExpressionContext self, string key = "MainDb") where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return new SelectProvider5<T1, T2, T3, T4, T5>(key, ins!.GetContext, ins!.GetDbInfo(key), ins!.Life);
    }

    public static IExpSelect<T1, T2, T3, T4, T5, T6> Select<T1, T2, T3, T4, T5, T6>(this IExpressionContext self, string key = "MainDb") where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return new SelectProvider6<T1, T2, T3, T4, T5, T6>(key, ins!.GetContext, ins!.GetDbInfo(key), ins!.Life);
    }

    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7> Select<T1, T2, T3, T4, T5, T6, T7>(this IExpressionContext self, string key = "MainDb") where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return new SelectProvider7<T1, T2, T3, T4, T5, T6, T7>(key, ins!.GetContext, ins!.GetDbInfo(key), ins!.Life);
    }

    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> Select<T1, T2, T3, T4, T5, T6, T7, T8>(this IExpressionContext self, string key = "MainDb") where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return new SelectProvider8<T1, T2, T3, T4, T5, T6, T7, T8>(key, ins!.GetContext, ins!.GetDbInfo(key), ins!.Life);
    }

    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IExpressionContext self, string key = "MainDb") where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return new SelectProvider9<T1, T2, T3, T4, T5, T6, T7, T8, T9>(key, ins!.GetContext, ins!.GetDbInfo(key), ins!.Life);
    }

    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this IExpressionContext self, string key = "MainDb") where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return new SelectProvider10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(key, ins!.GetContext, ins!.GetDbInfo(key), ins!.Life);
    }

    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this IExpressionContext self, string key = "MainDb") where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return new SelectProvider11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(key, ins!.GetContext, ins!.GetDbInfo(key), ins!.Life);
    }

    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this IExpressionContext self, string key = "MainDb") where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return new SelectProvider12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(key, ins!.GetContext, ins!.GetDbInfo(key), ins!.Life);
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this IExpressionContext self, string key = "MainDb") where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return new SelectProvider13<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(key, ins!.GetContext, ins!.GetDbInfo(key), ins!.Life);
    }

    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this IExpressionContext self, string key = "MainDb") where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return new SelectProvider14<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(key, ins!.GetContext, ins!.GetDbInfo(key), ins!.Life);
    }

    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this IExpressionContext self, string key = "MainDb") where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return new SelectProvider15<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(key, ins!.GetContext, ins!.GetDbInfo(key), ins!.Life);
    }

    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this IExpressionContext self, string key = "MainDb") where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return new SelectProvider16<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(key, ins!.GetContext, ins!.GetDbInfo(key), ins!.Life);
    }
}
