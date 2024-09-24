namespace LightORM;

public static partial class SelectExtension
{

    static string? GetDbKey(params Type[] types)
    {
        List<string> keys = new List<string>();
        foreach (var item in types)
        {
            var t = TableContext.GetTableInfo(item);
            if (t.TargetDatabase == null)
            {
                continue;
            }
            keys.Add(t.TargetDatabase);
        }
        var dbKeys = keys.Distinct().ToArray();
        if (dbKeys.Length > 1)
        {
            throw new LightOrmException($"不能设置不同的目标数据库: {string.Join(", ", dbKeys)}");
        }
        return dbKeys.FirstOrDefault();
    }

    #region 2个类型参数

    public static IExpSelect<T1, T2> Select<T1, T2>(this IExpressionContext instance)
    {
        var key = GetDbKey(typeof(T1), typeof(T2));
        if (key != null)
            instance.SwitchDatabase(key);
        return new SelectProvider2<T1, T2>(instance.Ado);
    }

    #endregion
#if NET45_OR_GREATER

    #region 3个类型参数

    public static IExpSelect<T1, T2, T3> Select<T1, T2, T3>(this IExpressionContext instance) where T1 : class, new()
    {
        var key = GetDbKey(typeof(T1), typeof(T2), typeof(T3));
        if (key != null)
            instance.SwitchDatabase(key);
        return new SelectProvider3<T1, T2, T3>(instance.Ado);
    }

    #endregion
    #region 4个类型参数
    public static IExpSelect<T1, T2, T3, T4> Select<T1, T2, T3, T4>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4>((t1, t2, t3, t4) => new { t1, t2, t3, t4 });
    }
    public static IExpSelect<T1, T2, T3, T4> Select<T1, T2, T3, T4>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4>(exp, ins!);
    }
    public static IExpSelect<T1, T2, T3, T4> Select<T1, T2, T3, T4>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4>(exp, ins!);
    }
    static SelectProvider4<T1, T2, T3, T4> CreateProvider<T1, T2, T3, T4>(Expression exp, ExpressionCoreSql instance)
    {
        var key = GetDbKey(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        if (key != null)
            instance.SwitchDatabase(key);
        return new SelectProvider4<T1, T2, T3, T4>(exp, instance.Ado);
    }
    #endregion

    #region 5个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5> Select<T1, T2, T3, T4, T5>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5>((t1, t2, t3, t4, t5) => new { t1, t2, t3, t4, t5 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5> Select<T1, T2, T3, T4, T5>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5>(exp, ins!);
    }
    public static IExpSelect<T1, T2, T3, T4, T5> Select<T1, T2, T3, T4, T5>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5>(exp, ins!);
    }
    static SelectProvider5<T1, T2, T3, T4, T5> CreateProvider<T1, T2, T3, T4, T5>(Expression exp, ExpressionCoreSql instance)
    {
        var key = GetDbKey(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        if (key != null)
            instance.SwitchDatabase(key);
        return new SelectProvider5<T1, T2, T3, T4, T5>(exp, instance.Ado);
    }
    #endregion

    #region 6个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6> Select<T1, T2, T3, T4, T5, T6>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6>((t1, t2, t3, t4, t5, t6) => new { t1, t2, t3, t4, t5, t6 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6> Select<T1, T2, T3, T4, T5, T6>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6>(exp, ins!);
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6> Select<T1, T2, T3, T4, T5, T6>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6>(exp, ins!);
    }
    static SelectProvider6<T1, T2, T3, T4, T5, T6> CreateProvider<T1, T2, T3, T4, T5, T6>(Expression exp, ExpressionCoreSql instance)
    {
        var key = GetDbKey(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        if (key != null)
            instance.SwitchDatabase(key);
        return new SelectProvider6<T1, T2, T3, T4, T5, T6>(exp, instance.Ado);
    }
    #endregion

    #region 7个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7> Select<T1, T2, T3, T4, T5, T6, T7>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7>((t1, t2, t3, t4, t5, t6, t7) => new { t1, t2, t3, t4, t5, t6, t7 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7> Select<T1, T2, T3, T4, T5, T6, T7>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7>(exp, ins!);
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7> Select<T1, T2, T3, T4, T5, T6, T7>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7>(exp, ins!);
    }
    static SelectProvider7<T1, T2, T3, T4, T5, T6, T7> CreateProvider<T1, T2, T3, T4, T5, T6, T7>(Expression exp, ExpressionCoreSql instance)
    {
        var key = GetDbKey(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        if (key != null)
            instance.SwitchDatabase(key);
        return new SelectProvider7<T1, T2, T3, T4, T5, T6, T7>(exp, instance.Ado);
    }
    #endregion

    #region 8个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> Select<T1, T2, T3, T4, T5, T6, T7, T8>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7, T8>((t1, t2, t3, t4, t5, t6, t7, t8) => new { t1, t2, t3, t4, t5, t6, t7, t8 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> Select<T1, T2, T3, T4, T5, T6, T7, T8>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8>(exp, ins!);
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8> Select<T1, T2, T3, T4, T5, T6, T7, T8>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8>(exp, ins!);
    }
    static SelectProvider8<T1, T2, T3, T4, T5, T6, T7, T8> CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8>(Expression exp, ExpressionCoreSql instance)
    {
        var key = GetDbKey(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        if (key != null)
            instance.SwitchDatabase(key);
        return new SelectProvider8<T1, T2, T3, T4, T5, T6, T7, T8>(exp, instance.Ado);
    }
    #endregion

    #region 9个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7, T8, T9>((t1, t2, t3, t4, t5, t6, t7, t8, t9) => new { t1, t2, t3, t4, t5, t6, t7, t8, t9 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9>(exp, ins!);
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9>(exp, ins!);
    }
    static SelectProvider9<T1, T2, T3, T4, T5, T6, T7, T8, T9> CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Expression exp, ExpressionCoreSql instance)
    {
        var key = GetDbKey(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        if (key != null)
            instance.SwitchDatabase(key);
        return new SelectProvider9<T1, T2, T3, T4, T5, T6, T7, T8, T9>(exp, instance.Ado);
    }
    #endregion

    #region 10个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>((t1, t2, t3, t4, t5, t6, t7, t8, t9, t10) => new { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(exp, ins!);
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(exp, ins!);
    }
    static SelectProvider10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression exp, ExpressionCoreSql instance)
    {
        var key = GetDbKey(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        if (key != null)
            instance.SwitchDatabase(key);
        return new SelectProvider10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(exp, instance.Ado);
    }
    #endregion

    #region 11个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>((t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11) => new { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(exp, ins!);
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(exp, ins!);
    }
    static SelectProvider11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Expression exp, ExpressionCoreSql instance)
    {
        var key = GetDbKey(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11));
        if (key != null)
            instance.SwitchDatabase(key);
        return new SelectProvider11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(exp, instance.Ado);
    }
    #endregion

    #region 12个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>((t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12) => new { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(exp, ins!);
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(exp, ins!);
    }
    static SelectProvider12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Expression exp, ExpressionCoreSql instance)
    {
        var key = GetDbKey(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12));
        if (key != null)
            instance.SwitchDatabase(key);
        return new SelectProvider12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(exp, instance.Ado);
    }
    #endregion

    #region 13个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>((t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13) => new { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(exp, ins!);
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(exp, ins!);
    }
    static SelectProvider13<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Expression exp, ExpressionCoreSql instance)
    {
        var key = GetDbKey(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13));
        if (key != null)
            instance.SwitchDatabase(key);
        return new SelectProvider13<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(exp, instance.Ado);
    }
    #endregion

    #region 14个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>((t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14) => new { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(exp, ins!);
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(exp, ins!);
    }
    static SelectProvider14<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Expression exp, ExpressionCoreSql instance)
    {
        var key = GetDbKey(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14));
        if (key != null)
            instance.SwitchDatabase(key);
        return new SelectProvider14<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(exp, instance.Ado);
    }
    #endregion

    #region 15个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>((t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15) => new { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(exp, ins!);
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(exp, ins!);
    }
    static SelectProvider15<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Expression exp, ExpressionCoreSql instance)
    {
        var key = GetDbKey(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15));
        if (key != null)
            instance.SwitchDatabase(key);
        return new SelectProvider15<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(exp, instance.Ado);
    }
    #endregion

    #region 16个类型参数
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this IExpressionContext self) where T1 : class, new()
    {
        return self.Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>((t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16) => new { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16 });
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this IExpressionContext self, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(exp, ins!);
    }
    public static IExpSelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Select<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this IExpressionContext self, Expression<Func<TypeSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, object>> exp) where T1 : class, new()
    {
        var ins = self as ExpressionCoreSql;
        return CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(exp, ins!);
    }
    static SelectProvider16<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> CreateProvider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Expression exp, ExpressionCoreSql instance)
    {
        var key = GetDbKey(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(T16));
        if (key != null)
            instance.SwitchDatabase(key);
        return new SelectProvider16<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(exp, instance.Ado);
    }
    #endregion

#endif
}

