namespace LightORM;

public static partial class SelectExtensions
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
            LightOrmException.Throw($"不能设置不同的目标数据库: {string.Join(", ", dbKeys)}");
        }
        return dbKeys.FirstOrDefault();
    }


    public static IEnumerable<dynamic> ToDynamicList<T1>(this IExpSelect<T1> select, Expression<Func<T1, object>> exp)
    {
        select.HandleResult(exp, null);
        return select.ToList<MapperRow>();
    }

    public static async Task<IList<dynamic>> ToDynamicListAsync<T1>(this IExpSelect<T1> select, Expression<Func<T1, object>> exp)
    {
        select.HandleResult(exp, null);
        var list = await select.ToListAsync<MapperRow>();
        return list.Cast<dynamic>().ToList();
    }

    public static DataTable ToDataTable<T1>(this IExpSelect<T1> select, Expression<Func<T1, object>> exp)
    {
        select.HandleResult(exp, null);
        var sql = select.SqlBuilder.ToSqlString();
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.ExecuteDataTable(sql, parameters);
    }

    public static Task<DataTable> ToDataTableAsync<T1>(this IExpSelect<T1> select, Expression<Func<T1, object>> exp)
    {
        select.HandleResult(exp, null);
        var sql = select.SqlBuilder.ToSqlString();
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.ExecuteDataTableAsync(sql, parameters);
    }


    #region 2个类型参数

    public static IExpSelect<T1, T2> Select<T1, T2>(this IExpressionContext instance)
    {
        var key = GetDbKey(typeof(T1), typeof(T2));
        if (key != null)
            instance.SwitchDatabase(key);
        return new SelectProvider2<T1, T2>(instance.Ado);
    }

    public static IExpSelect<T1, T2> InnerJoin<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<T1, T2, bool>> on)
    {
        select.JoinHandle(on, TableLinkType.InnerJoin);
        return select;
    }

    public static IExpSelect<T1, T2> InnerLeft<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<T1, T2, bool>> on)
    {
        select.JoinHandle(on, TableLinkType.LeftJoin);
        return select;
    }

    public static IExpSelect<T1, T2> InnerRight<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<T1, T2, bool>> on)
    {
        select.JoinHandle(on, TableLinkType.RightJoin);
        return select;
    }

    public static IEnumerable<dynamic> ToDynamicList<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<T1, T2, object>> exp)
    {
        select.HandleResult(exp, null);
        return select.ToList<MapperRow>();
    }

    public static async Task<IList<dynamic>> ToDynamicListAsync<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<T1, T2, object>> exp)
    {
        select.HandleResult(exp, null);
        var list = await select.ToListAsync<MapperRow>();
        return list.Cast<dynamic>().ToList();
    }

    public static DataTable ToDataTable<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<T1, T2, object>> exp)
    {
        select.HandleResult(exp, null);
        var sql = select.SqlBuilder.ToSqlString();
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.ExecuteDataTable(sql, parameters);
    }

    public static Task<DataTable> ToDataTableAsync<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<T1, T2, object>> exp)
    {
        select.HandleResult(exp, null);
        var sql = select.SqlBuilder.ToSqlString();
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.ExecuteDataTableAsync(sql, parameters);
    }

    #region TypeSet

    public static IExpSelect<T1, T2> InnerJoin<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<TypeSet<T1, T2>, bool>> on)
    {
        var flatExp = FlatTypeSet.Default.Flat(on)!;
        select.JoinHandle(flatExp, TableLinkType.InnerJoin);
        return select;
    }

    public static IExpSelect<T1, T2> InnerLeft<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<TypeSet<T1, T2>, bool>> on)
    {
        var flatExp = FlatTypeSet.Default.Flat(on)!;
        select.JoinHandle(flatExp, TableLinkType.LeftJoin);
        return select;
    }

    public static IExpSelect<T1, T2> InnerRight<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<TypeSet<T1, T2>, bool>> on)
    {
        var flatExp = FlatTypeSet.Default.Flat(on)!;
        select.JoinHandle(flatExp, TableLinkType.RightJoin);
        return select;
    }

    public static IEnumerable<dynamic> ToDynamicList<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<TypeSet<T1, T2>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        select.HandleResult(flatExp, null);
        return select.ToList<MapperRow>();
    }

    public static async Task<IList<dynamic>> ToDynamicListAsync<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<TypeSet<T1, T2>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        select.HandleResult(flatExp, null);
        var list = await select.ToListAsync<MapperRow>();
        return list.Cast<dynamic>().ToList();
    }

    public static DataTable ToDataTable<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<TypeSet<T1, T2>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        select.HandleResult(flatExp, null);
        var sql = select.SqlBuilder.ToSqlString();
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.ExecuteDataTable(sql, parameters);
    }

    public static Task<DataTable> ToDataTableAsync<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<TypeSet<T1, T2>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        select.HandleResult(flatExp, null);
        var sql = select.SqlBuilder.ToSqlString();
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.ExecuteDataTableAsync(sql, parameters);
    }

    #endregion

    #endregion

}

