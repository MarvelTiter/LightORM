using LightORM.Extension;
using LightORM.Providers;
using System.Diagnostics.CodeAnalysis;
namespace LightORM;

public static class IncludeExtensions
{
    public static IExpInclude<T1, TMember> ThenInclude<
#if NET8_0_OR_GREATER
       [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    T1, TElement, TMember>(this IExpInclude<T1, IEnumerable<TElement>> include, Expression<Func<TElement, TMember>> exp)
    {
        //var option = SqlResolveOptions.Select;
        //var result = exp.Resolve(option, ResolveContext.Create(Executor.Database.DbBaseType));
        var p = (IncludeProvider<T1, TElement>)include;
        string? includePropertyName = null;
        Expression? includeWhereExpression = null;
        if (exp is MemberExpression m)
        {
            includePropertyName = m.Member.Name;
        }
        else if (exp is MethodCallExpression mc)
        {
            var member = mc.Arguments[0] as MemberExpression;
            includePropertyName = member?.Member.Name;
            if (mc.Arguments.Count > 1)
            {
                includeWhereExpression = mc.Arguments[1];
            }
        }
        if (string.IsNullOrEmpty(includePropertyName))
        {
            throw new LightOrmException("解析导航属性失败");
        }

        var last = include.SqlBuilder.Includes.Last();
        var lastIncludeTable = TableContext.GetTableInfo(last.NavigateInfo!.NavigateType);
        
        var navCol = lastIncludeTable.GetColumn(includePropertyName!)!;
        var navInfo = navCol.NavigateInfo!;
        //var table = TableInfo.Create(navCol.NavigateInfo!.NavigateType);
        var parentWhereColumn = lastIncludeTable.GetColumn(navCol.NavigateInfo!.MainName!);
        var includeInfo = new IncludeInfo
        {
            NavigateInfo = navInfo,
            ParentNavigateColumn = navCol,
            ParentWhereColumn = parentWhereColumn,
            ParentTable = include.SqlBuilder.MainTable,
            IncludeWhereExpression = includeWhereExpression
        };
        last.ThenIncludes.Add(includeInfo);
        return new IncludeProvider<T1, TMember>(include.DbContext, include.SqlBuilder);
    }

    public static IExpInclude<T1, TMember> ThenInclude<
#if NET8_0_OR_GREATER
       [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    T1, TElement, TMember>(this IExpInclude<T1, TElement> include, Expression<Func<TElement, TMember>> exp)
    {
        var p = (IncludeProvider<T1, TElement>)include;
        //TODO 处理 ThenInclude
        return new IncludeProvider<T1, TMember>(p.DbContext, p.SqlBuilder);
    }

    public static bool WhereIf<T>(this IEnumerable<T> values, Expression<Func<T, bool>> predicate)
    {
        return true;
    }

}
