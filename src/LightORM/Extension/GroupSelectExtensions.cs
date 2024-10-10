using System.CodeDom.Compiler;

namespace LightORM;

public static class GroupSelectExtensions
{
    public static DataTable ToDataTable<TGroup, TTables>(this IExpSelectGroup<TGroup, TTables> selectGroup)
    {

        var sql = selectGroup.SqlBuilder.ToSqlString();
        var dbParams = selectGroup.SqlBuilder.DbParameters;
        return selectGroup.Executor.ExecuteDataTable(sql, dbParams);
    }

    public static DataTable ToDataTable<TGroup, TTables>(this IExpSelectGroup<TGroup, TTables> selectGroup, Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp);
        selectGroup.SqlBuilder.Expressions.Add(new ExpressionInfo()
        {
            Expression = flatExp,
            ResolveOptions = SqlResolveOptions.Select
        });
        var sql = selectGroup.SqlBuilder.ToSqlString();
        var dbParams = selectGroup.SqlBuilder.DbParameters;
        return selectGroup.Executor.ExecuteDataTable(sql, dbParams);
    }

    public static Task<DataTable> ToDataTableAsync<TGroup, TTables>(this IExpSelectGroup<TGroup, TTables> selectGroup)
    {
        var sql = selectGroup.SqlBuilder.ToSqlString();
        var dbParams = selectGroup.SqlBuilder.DbParameters;
        return selectGroup.Executor.ExecuteDataTableAsync(sql, dbParams);
    }

    public static Task<DataTable> ToDataTableAsync<TGroup, TTables>(this IExpSelectGroup<TGroup, TTables> selectGroup, Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp);
        selectGroup.SqlBuilder.Expressions.Add(new ExpressionInfo()
        {
            Expression = flatExp,
            ResolveOptions = SqlResolveOptions.Select
        });
        var sql = selectGroup.SqlBuilder.ToSqlString();
        var dbParams = selectGroup.SqlBuilder.DbParameters;
        return selectGroup.Executor.ExecuteDataTableAsync(sql, dbParams);
    }
}

