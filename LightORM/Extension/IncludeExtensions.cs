using LightORM.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM;

public static class IncludeExtensions
{
    public static IExpInclude<T1, TMember> ThenInclude<T1, TElement, TMember>(this IExpInclude<T1, IEnumerable<TElement>> include, Expression<Func<TElement, TMember>> exp)
    {
        var p = (IncludeProvider<T1, TElement>)include;
        //TODO 处理 ThenInclude
        return new IncludeProvider<T1, TMember>(p.SqlExecutor);
    }

    public static IExpInclude<T1, TMember> ThenInclude<T1, TElement, TMember>(this IExpInclude<T1, TElement> include, Expression<Func<TElement, TMember>> exp)
    {
        var p = (IncludeProvider<T1, TElement>)include;
        //TODO 处理 ThenInclude
        return new IncludeProvider<T1, TMember>(p.SqlExecutor);
    }
}
