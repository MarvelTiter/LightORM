using LightORM.Utils.Vistors;

namespace LightORM;

public static class GroupSelectExtensions
{
    public static IExpSelectGroup<TGroup, TTables> Result<TGroup, TTables>(this IExpSelectGroup<TGroup, TTables> selectGroup, Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
    {
        var flatExp = FlatGrouping.Default.Flat(exp, selectGroup.KeySelector) ?? throw new LightOrmException("表达式扁平化失败");
        selectGroup.SqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.Select, flatExp));
        return selectGroup;
    }
    public static DataTable ToDataTable<TGroup, TTables>(this IExpSelectGroup<TGroup, TTables> selectGroup)
    {

        var sql = selectGroup.SqlBuilder.ToSqlString(selectGroup.Executor.Database.DatabaseAdapter);
        var dbParams = selectGroup.SqlBuilder.DbParameters;
        return selectGroup.Executor.ExecuteDataTable(sql, dbParams);
    }

    public static DataTable ToDataTable<TGroup, TTables>(this IExpSelectGroup<TGroup, TTables> selectGroup, Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
    {
        var flatExp = FlatGrouping.Default.Flat(exp, selectGroup.KeySelector) ?? throw new LightOrmException("表达式扁平化失败");
        selectGroup.SqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.Select, flatExp));
        var sql = selectGroup.SqlBuilder.ToSqlString(selectGroup.Executor.Database.DatabaseAdapter);
        var dbParams = selectGroup.SqlBuilder.DbParameters;
        return selectGroup.Executor.ExecuteDataTable(sql, dbParams);
    }

    public static Task<DataTable> ToDataTableAsync<TGroup, TTables>(this IExpSelectGroup<TGroup, TTables> selectGroup)
    {
        var sql = selectGroup.SqlBuilder.ToSqlString(selectGroup.Executor.Database.DatabaseAdapter);
        var dbParams = selectGroup.SqlBuilder.DbParameters;
        return selectGroup.Executor.ExecuteDataTableAsync(sql, dbParams);
    }

    public static Task<DataTable> ToDataTableAsync<TGroup, TTables>(this IExpSelectGroup<TGroup, TTables> selectGroup, Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
    {
        var flatExp = FlatGrouping.Default.Flat(exp, selectGroup.KeySelector) ?? throw new LightOrmException("表达式扁平化失败");
        selectGroup.SqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.Select, flatExp));
        var sql = selectGroup.SqlBuilder.ToSqlString(selectGroup.Executor.Database.DatabaseAdapter);
        var dbParams = selectGroup.SqlBuilder.DbParameters;
        return selectGroup.Executor.ExecuteDataTableAsync(sql, dbParams);
    }
}

