using LightORM.ExpressionSql;
using LightORM.Providers.Select;
using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;

namespace LightORM.Extension;

public static class SelectExtension
{

    #region 2个类型参数
    public static IExpSelect<T1, T2> Select<T1, T2>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2>((t1, t2) => new { t1, t2 });
    }
    public static IExpSelect<T1, T2> Select<T1, T2>(this IExpressionContext self, Expression<Func<T1, T2, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    static IExpSelect<T1, T2> CreateProvider<T1, T2>(Expression exp, ISqlExecutor executor) => new SelectProvider2<T1, T2>(exp, executor);
    #endregion

    #region 3个类型参数
    public static IExpSelect<T1, T2, T3> Select<T1, T2, T3>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3>((t1, t2, t3) => new { t1, t2, t3 });
    }
    public static IExpSelect<T1, T2, T3> Select<T1, T2, T3>(this IExpressionContext self, Expression<Func<T1, T2, T3, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    public static IExpSelect<T1, T2, T3> Select<T1, T2, T3>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    static IExpSelect<T1, T2, T3> CreateProvider<T1, T2, T3>(Expression exp, ISqlExecutor executor) => new SelectProvider3<T1, T2, T3>(exp, executor);
    #endregion

    #region 4个类型参数
    public static IExpSelect<T1, T2, T3, T4> Select<T1, T2, T3, T4>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4>((t1, t2, t3, t4) => new { t1, t2, t3, t4 });
    }
    public static IExpSelect<T1, T2, T3, T4> Select<T1, T2, T3, T4>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    public static IExpSelect<T1, T2, T3, T4> Select<T1, T2, T3, T4>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    static IExpSelect<T1, T2, T3, T4> CreateProvider<T1, T2, T3, T4>(Expression exp, ISqlExecutor executor) => new SelectProvider4<T1, T2, T3, T4>(exp, executor);
    #endregion

    #region 5个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5> Select<T1, T2, T3, T4, T5>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5>((t1, t2, t3, t4, t5) => new { t1, t2, t3, t4, t5 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5> Select<T1, T2, T3, T4, T5>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    public static IExpSelect<T1, T2, T3, T4, T5> Select<T1, T2, T3, T4, T5>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    static IExpSelect<T1, T2, T3, T4, T5> CreateProvider<T1, T2, T3, T4, T5>(Expression exp, ISqlExecutor executor) => new SelectProvider5<T1, T2, T3, T4, T5>(exp, executor);
    #endregion

    #region 6个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6> Select<T1, T2, T3, T4, T5, T6>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6>((t1, t2, t3, t4, t5, t6) => new { t1, t2, t3, t4, t5, t6 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6> Select<T1, T2, T3, T4, T5, T6>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6> Select<T1, T2, T3, T4, T5, T6>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    static IExpSelect<T1, T2, T3, T4, T5, T6> CreateProvider<T1, T2, T3, T4, T5, T6>(Expression exp, ISqlExecutor executor) => new SelectProvider6<T1, T2, T3, T4, T5, T6>(exp, executor);
    #endregion

    #region 7个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7> Select<T1, T2, T3, T4, T5, T6, T7>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7>((t1, t2, t3, t4, t5, t6, t7) => new { t1, t2, t3, t4, t5, t6, t7 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7> Select<T1, T2, T3, T4, T5, T6, T7>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7> Select<T1, T2, T3, T4, T5, T6, T7>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    static IExpSelect<T1, T2, T3, T4, T5, T6, T7> CreateProvider<T1, T2, T3, T4, T5, T6, T7>(Expression exp, ISqlExecutor executor) => new SelectProvider7<T1, T2, T3, T4, T5, T6, T7>(exp, executor);
    #endregion

    #region 8个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> Select<T1, T2, T3, T4, T5, T6, T7, T8>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7, T8>((t1, t2, t3, t4, t5, t6, t7, t8) => new { t1, t2, t3, t4, t5, t6, t7, t8 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> Select<T1, T2, T3, T4, T5, T6, T7, T8>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> Select<T1, T2, T3, T4, T5, T6, T7, T8>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8>(Expression exp, ISqlExecutor executor) => new SelectProvider8<T1, T2, T3, T4, T5, T6, T7, T8>(exp, executor);
    #endregion

    #region 9个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7, T8, T9>((t1, t2, t3, t4, t5, t6, t7, t8, t9) => new { t1, t2, t3, t4, t5, t6, t7, t8, t9 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Expression exp, ISqlExecutor executor) => new SelectProvider9<T1, T2, T3, T4, T5, T6, T7, T8, T9>(exp, executor);
    #endregion

    #region 10个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>((t1, t2, t3, t4, t5, t6, t7, t8, t9, t10) => new { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression exp, ISqlExecutor executor) => new SelectProvider10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(exp, executor);
    #endregion

    #region 11个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>((t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11) => new { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Expression exp, ISqlExecutor executor) => new SelectProvider11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(exp, executor);
    #endregion

    #region 12个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>((t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12) => new { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Expression exp, ISqlExecutor executor) => new SelectProvider12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(exp, executor);
    #endregion

    #region 13个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>((t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13) => new { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Expression exp, ISqlExecutor executor) => new SelectProvider13<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(exp, executor);
    #endregion

    #region 14个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>((t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14) => new { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Expression exp, ISqlExecutor executor) => new SelectProvider14<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(exp, executor);
    #endregion

    #region 15个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>((t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15) => new { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Expression exp, ISqlExecutor executor) => new SelectProvider15<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(exp, executor);
    #endregion

    #region 16个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>((t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16) => new { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(exp, ExpressionCoreSql.GetExecutor(ins!.CurrentKey));
    }
    static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Expression exp, ISqlExecutor executor) => new SelectProvider16<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(exp, executor);
    #endregion

}
