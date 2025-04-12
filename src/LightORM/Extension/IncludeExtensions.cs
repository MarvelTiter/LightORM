using LightORM.Providers;
using LightORM.Extension;
namespace LightORM;

public static class IncludeExtensions
{

    public static IExpInclude<T1, TMember> ThenInclude<T1, TElement, TMember>(this IExpInclude<T1, IEnumerable<TElement>> include, Expression<Func<TElement, TMember>> exp)
    {
        var option = SqlResolveOptions.Select;
        //option.DbType = include.SqlBuilder.DbType;
        var result = exp.Resolve(option, ResolveContext.Create(include.Executor.Database.DbBaseType));
        var parentTable = TableInfo.Create<TElement>();
        var navCol = parentTable.GetColumnInfo(result.NavigateMembers!.First());
        var navInfo = navCol.NavigateInfo!;
        var table = TableInfo.Create(navInfo.NavigateType);
        var parentWhereColumn = parentTable.GetColumnInfo(navInfo.MainName!);
        var includeInfo = new IncludeInfo
        {
            SelectedTable = table,
            NavigateInfo = navInfo,
            ParentNavigateColumn = navCol,
            ParentWhereColumn = parentWhereColumn,
            ExpressionResolvedResult = result
        };
        include.SqlBuilder.IncludeContext.ThenInclude ??= new IncludeContext(include.Executor.Database.DbBaseType);
        include.SqlBuilder.IncludeContext.ThenInclude.Includes.Add(includeInfo);
        return new IncludeProvider<T1, TMember>(include.Executor, include.SqlBuilder);
    }

    public static IExpInclude<T1, TMember> ThenInclude<T1, TElement, TMember>(this IExpInclude<T1, TElement> include, Expression<Func<TElement, TMember>> exp)
    {
        var p = (IncludeProvider<T1, TElement>)include;
        //TODO 处理 ThenInclude
        return new IncludeProvider<T1, TMember>(p.Executor, p.SqlBuilder);
    }

    public static bool WhereIf<T>(this IEnumerable<T> values, Expression<Func<T, bool>> predicate)
    {
        return true;
    }

}
