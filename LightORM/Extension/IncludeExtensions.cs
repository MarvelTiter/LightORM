using LightORM.Builder;
using LightORM.Cache;
using LightORM.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightORM.Extension;
using LightORM.Interfaces.ExpSql;
namespace LightORM;

public static class IncludeExtensions
{
    //public static IExpInclude<T1, TMember> Include<T1, TMember>(this IExpSelect<T1> select, Expression<Func<T1, bool>> exp)
    //{
    //    var provider = (SelectProvider1<T1>)select;
    //    return provider.Include<T1, TMember>(exp);
    //}

    //internal static IExpInclude<T1, TMember> Include<T1, TMember>(this SelectProvider1<T1> provider, Expression<Func<T1, bool>> exp)
    //{
    //    return provider.CreateIncludeProvider<TMember>(exp);
    //}

    public static IExpInclude<T1, TMember> ThenInclude<T1, TElement, TMember>(this IExpInclude<T1, IEnumerable<TElement>> include, Expression<Func<TElement, TMember>> exp)
    {
        //TODO 处理 ThenInclude
        var option = SqlResolveOptions.Select;
        option.DbType = include.SqlBuilder.DbType;
        var result = exp.Resolve(option);
        var parentTable = TableContext.GetTableInfo(typeof(TElement));
        var navCol = parentTable.GetColumnInfo(result.NavigateMembers!.First());
        var navInfo = navCol.NavigateInfo!;
        var table = TableContext.GetTableInfo(navInfo.NavigateType);
        var parentWhereColumn = parentTable.GetColumnInfo(navInfo.MainName!);
        var includeInfo = new IncludeInfo
        {
            SelectedTable = table,
            NavigateInfo = navInfo,
            ParentNavigateColumn = navCol,
            ParentWhereColumn = parentWhereColumn,
            ExpressionResolvedResult = result
        };
        include.SqlBuilder.IncludeContext.ThenInclude ??= new IncludeContext(include.Executor.ConnectInfo.DbBaseType);
        include.SqlBuilder.IncludeContext.ThenInclude.Includes.Add(includeInfo);
        return new IncludeProvider<T1, TMember>(include.Executor, include.SqlBuilder);
    }

    //static IncludeContext FindIncludeContext(IncludeContext context)
    //{
    //    if (context.ThenInclude == null)
    //    {
    //        return context;
    //    }
    //    return FindIncludeContext(context.ThenInclude);
    //}

    public static IExpInclude<T1, TMember> ThenInclude<T1, TElement, TMember>(this IExpInclude<T1, TElement> include, Expression<Func<TElement, TMember>> exp)
    {
        var p = (IncludeProvider<T1, TElement>)include;
        //TODO 处理 ThenInclude
        return new IncludeProvider<T1, TMember>(p.Executor, p.SqlBuilder);
    }

    public static bool When<T>(this IEnumerable<T> values, Func<T, bool> predicate)
    {
        return true;
    }

}
