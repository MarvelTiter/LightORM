using LightORM.Utils.Vistors;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace LightORM;

// public static partial class Select1Ex<T1>
// {
//     public static IExpSelect<T1, TJoin> InnerJoin<TJoin>(this IExpSelect)
// }

public static partial class SelectExtensions
{
    private static string? GetDbKey(params Type[] types)
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


    extension<T1>(IExpSelect<T1> select)
    {
        public IEnumerable<dynamic> ToDynamicList(Expression<Func<T1, object>> exp)
        {
            select.HandleResult(exp, null);
            return select.ToList<MapperRow>();
        }

        public async Task<IList<dynamic>> ToDynamicListAsync(Expression<Func<T1, object>> exp, CancellationToken cancellationToken = default)
        {
            select.HandleResult(exp, null);
            var list = await select.ToListAsync<MapperRow>(cancellationToken);
            return [.. list.Cast<dynamic>()];
        }

        public DataTable ToDataTable(Expression<Func<T1, object>> exp)
        {
            select.HandleResult(exp, null);
            var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
            var parameters = select.SqlBuilder.DbParameters;
            return select.Executor.ExecuteDataTable(sql, parameters);
        }

        public Task<DataTable> ToDataTableAsync(Expression<Func<T1, object>> exp, CancellationToken cancellationToken = default)
        {
            select.HandleResult(exp, null);
            var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
            var parameters = select.SqlBuilder.DbParameters;
            return select.Executor.ExecuteDataTableAsync(sql, parameters, cancellationToken: cancellationToken);
        }

        public IExpSelect<T1> SelectColumns(Expression<Func<T1, object>> exp)
        {
            select.HandleResult(exp, null);
            return select;
        }
    }

    #region 2个类型参数

    public static IExpSelect<T1, T2> Select<
#if NET8_0_OR_GREATER
       [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    T1, T2>(this IExpressionContext instance)
    {
        var key = GetDbKey(typeof(T1), typeof(T2));
        if (key != null)
        {
            return new SelectProvider2<T1, T2>(instance.SwitchDatabase(key));
        }

        return new SelectProvider2<T1, T2>(instance);
    }

    extension<T1, T2>(IExpSelect<T1, T2> select)
    {
        public IExpSelect<T1, T2> SelectColumns(Expression<Func<T1, T2, object>> exp)
        {
            select.HandleResult(exp, null);
            return select;
        }

        /// <summary>
        /// 条件Where
        /// </summary>
        public IExpSelect<T1, T2> WhereIf(bool condition, Expression<Func<T1, T2, bool>> exp)
        {
            if (condition)
            {
                select.Where(exp);
            }

            return select;
        }

        public IEnumerable<dynamic> ToDynamicList(Expression<Func<T1, T2, object>> exp)
        {
            select.HandleResult(exp, null);
            return select.InternalToList<MapperRow>();
        }

        public async Task<IList<dynamic>> ToDynamicListAsync(Expression<Func<T1, T2, object>> exp, CancellationToken cancellationToken = default)
        {
            select.HandleResult(exp, null);
            var list = await select.InternalToListAsync<MapperRow>(cancellationToken: cancellationToken);
            return [.. list.Cast<dynamic>()];
        }

        public DataTable ToDataTable(Expression<Func<T1, T2, object>> exp)
        {
            select.HandleResult(exp, null);
            var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
            var parameters = select.SqlBuilder.DbParameters;
            return select.Executor.ExecuteDataTable(sql, parameters);
        }

        public Task<DataTable> ToDataTableAsync(Expression<Func<T1, T2, object>> exp, CancellationToken cancellationToken = default)
        {
            select.HandleResult(exp, null);
            var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
            var parameters = select.SqlBuilder.DbParameters;
            return select.Executor.ExecuteDataTableAsync(sql, parameters, cancellationToken: cancellationToken);
        }

        #region TypeSet

        /// <summary>
        /// 条件Where
        /// </summary>
        public IExpSelect<T1, T2> WhereIf(bool condition, Expression<Func<TypeSet<T1, T2>, bool>> exp)
        {
            if (condition)
            {
                select.Where(exp);
            }

            return select;
        }

        public IEnumerable<dynamic> ToDynamicList(Expression<Func<TypeSet<T1, T2>, object>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp)!;
            select.HandleResult(flatExp, null);
            return select.InternalToList<MapperRow>();
        }

        public async Task<IList<dynamic>> ToDynamicListAsync(Expression<Func<TypeSet<T1, T2>, object>> exp, CancellationToken cancellationToken = default)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp)!;
            select.HandleResult(flatExp, null);
            var list = await select.InternalToListAsync<MapperRow>(cancellationToken: cancellationToken);
            return [.. list.Cast<dynamic>()];
        }

        public DataTable ToDataTable(Expression<Func<TypeSet<T1, T2>, object>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp)!;
            select.HandleResult(flatExp, null);
            var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
            var parameters = select.SqlBuilder.DbParameters;
            return select.Executor.ExecuteDataTable(sql, parameters);
        }

        public Task<DataTable> ToDataTableAsync(Expression<Func<TypeSet<T1, T2>, object>> exp, CancellationToken cancellationToken = default)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp)!;
            select.HandleResult(flatExp, null);
            var sql = select.SqlBuilder.ToSqlString(select.Executor.Database.DatabaseAdapter);
            var parameters = select.SqlBuilder.DbParameters;
            return select.Executor.ExecuteDataTableAsync(sql, parameters, cancellationToken: cancellationToken);
        }

        #endregion
    }

    #endregion
}