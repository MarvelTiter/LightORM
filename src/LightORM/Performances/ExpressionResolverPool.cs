using System.Collections.Concurrent;
using System.Text;
namespace LightORM.Performances;

internal static class ExpressionResolverPool
{
    private static readonly ConcurrentStack<ExpressionResolver> pool = [];
    public static ExpressionResolver Rent(SqlResolveOptions options, ResolveContext context)
    {
        if (pool.TryPop(out var resolver))
        {
            // 重置状态而不是创建新实例
            ResetResolver(resolver, options, context);
            return resolver;
        }
        return new ExpressionResolver(options, context);
    }

    public static void Return(ExpressionResolver resolver)
    {
        if (pool.Count < ExpressionSqlOptions.Instance.Value.InternalObjectPoolSize)
        {
            ClearResolver(resolver);
            pool.Push(resolver);
        }
    }

    private static void ResetResolver(ExpressionResolver resolver, SqlResolveOptions options, ResolveContext context)
    {
        // 重用StringBuilder，只需Clear
        resolver.Sql.Clear();
        resolver.Sql.EnsureCapacity(128); // 恢复初始容量

        // 清空列表，重用底层数组
        resolver.DbParameters.Clear();
        resolver.Members.Clear();
        resolver.ResolvedMembers.Clear();
        resolver.NavigateMembers?.Clear();
        resolver.WindowFnPartials?.Clear();

        // 重置字段
        resolver.Options = options;
        resolver.Context = context;
        resolver.IsNot = false;
        resolver.UseNavigate = false;
        resolver.NavigateDeep = 0;
        resolver.Parameters = null;
        resolver.ParameterPositionIndex = 0;
        resolver.ResolveNullValue = false;
        resolver.UseAs = true;
        resolver.IsVisitConvert = false;
        resolver.ContainVariable = false;
    }

    private static void ClearResolver(ExpressionResolver resolver)
    {
        // 清空但不释放大数组，如果太大，重建
        if (resolver.Sql.Capacity > 4096)
            resolver.Sql = new StringBuilder(128);

        if (resolver.DbParameters.Capacity > 32)
            resolver.DbParameters = new List<DbParameterInfo>(8);

        if (resolver.ResolvedMembers.Capacity > 16)
            resolver.ResolvedMembers = new List<string>(4);
    }
}