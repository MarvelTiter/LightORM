using LightORM.Utils.Vistors;

namespace LightORM;

// public static partial class Select1Ex<T1>
// {
//     public static IExpSelect<T1, TJoin> InnerJoin<TJoin>(this IExpSelect)
// }

public static partial class SelectExtensions
{
    static string? GetDbKey(params Type[] types)
    {
        HashSet<string> keys = [];
        foreach (var item in types)
        {
            var t = TableContext.GetTableInfo(item);
            if (t.TargetDatabase == null)
            {
                continue;
            }
            keys.Add(t.TargetDatabase);
        }
        if (keys.Count > 1)
        {
            throw new LightOrmException($"不能设置不同的目标数据库: {string.Join(", ", keys)}");
        }
        return keys.FirstOrDefault();
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
        var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.CustomDatabase);
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.ExecuteDataTable(sql, parameters);
    }

    public static Task<DataTable> ToDataTableAsync<T1>(this IExpSelect<T1> select, Expression<Func<T1, object>> exp)
    {
        select.HandleResult(exp, null);
        var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.CustomDatabase);
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.ExecuteDataTableAsync(sql, parameters);
    }

    public static IExpSelect<T1> SelectColumns<T1>(this IExpSelect<T1> select, Expression<Func<T1, object>> exp)
    {
        select.HandleResult(exp, null);
        return select;
    }
#if NET10_0_OR_GREATER
    
    extension<T1>(IExpSelect<T1> select)
    {
        
    }
#endif

    #region 2个类型参数

    public static IExpSelect<T1, T2> Select<T1, T2>(this IExpressionContext instance)
    {
        var key = GetDbKey(typeof(T1), typeof(T2));
        if (key != null)
        {
            return new SelectProvider2<T1, T2>(instance.GetAdo(key));
        }
        return new SelectProvider2<T1, T2>(instance.Ado);
    }

    public static IExpSelect<T1, T2> SelectColumns<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<T1, T2, object>> exp)
    {
        select.HandleResult(exp, null);
        return select;
    }

    /// <summary>
    /// 条件Where
    /// </summary>
    public static IExpSelect<T1, T2> WhereIf<T1, T2>(this IExpSelect<T1, T2> select, bool condition, Expression<Func<T1, T2, bool>> exp)
    {
        if (condition)
        {
            select.Where(exp);
        }
        return select;
    }

    public static IEnumerable<dynamic> ToDynamicList<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<T1, T2, object>> exp)
    {
        select.HandleResult(exp, null);
        return select.InternalToList<MapperRow>();
    }

    public static async Task<IList<dynamic>> ToDynamicListAsync<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<T1, T2, object>> exp)
    {
        select.HandleResult(exp, null);
        var list = await select.InternalToListAsync<MapperRow>();
        return [.. list.Cast<dynamic>()];
    }

    public static DataTable ToDataTable<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<T1, T2, object>> exp)
    {
        select.HandleResult(exp, null);
        var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.CustomDatabase);
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.ExecuteDataTable(sql, parameters);
    }

    public static Task<DataTable> ToDataTableAsync<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<T1, T2, object>> exp)
    {
        select.HandleResult(exp, null);
        var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.CustomDatabase);
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.ExecuteDataTableAsync(sql, parameters);
    }

    #region TypeSet

    /// <summary>
    /// 条件Where
    /// </summary>
    public static IExpSelect<T1, T2> WhereIf<T1, T2>(this IExpSelect<T1, T2> select, bool condition, Expression<Func<TypeSet<T1, T2>, bool>> exp)
    {
        if (condition)
        {
            select.Where(exp);
        }
        return select;
    }

    public static IEnumerable<dynamic> ToDynamicList<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<TypeSet<T1, T2>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        select.HandleResult(flatExp, null);
        return select.InternalToList<MapperRow>();
    }

    public static async Task<IList<dynamic>> ToDynamicListAsync<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<TypeSet<T1, T2>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        select.HandleResult(flatExp, null);
        var list = await select.InternalToListAsync<MapperRow>();
        return [.. list.Cast<dynamic>()];
    }

    public static DataTable ToDataTable<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<TypeSet<T1, T2>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        select.HandleResult(flatExp, null);
        var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.CustomDatabase);
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.ExecuteDataTable(sql, parameters);
    }

    public static Task<DataTable> ToDataTableAsync<T1, T2>(this IExpSelect<T1, T2> select, Expression<Func<TypeSet<T1, T2>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        select.HandleResult(flatExp, null);
        var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.CustomDatabase);
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.ExecuteDataTableAsync(sql, parameters);
    }

    #endregion

    #endregion

}

