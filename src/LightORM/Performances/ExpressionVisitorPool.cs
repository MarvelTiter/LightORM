using System.Collections.Concurrent;

namespace LightORM.Performances;

internal static class ExpressionVisitorPool<T>
    where T : ExpressionVisitor, IResetable, new()
{
    private static readonly ConcurrentStack<T> pool = new();
    public static T Rent()
    {
        if (pool.TryPop(out var visitor))
        {
            return visitor;
        }
        return new T();
    }

    public static void Return(T visitor)
    {
        if (pool.Count < ExpressionSqlOptions.Instance.Value.InternalObjectPoolSize)
        {
            visitor.Reset();
            pool.Push(visitor);
        }
    }

}
