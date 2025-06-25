namespace LightORM.Models;
internal static class Ex
{
    private static void ForEach<T>(this ICollection<T> collection, Action<T> action)
    {
        foreach (var item in collection)
        {
            action(item);
        }
    }
    public static void NotifyPrepareCommand(this AdoInterceptor interceptor, SqlExecuteContext context)
    {
        interceptor.Interceptors.ForEach(t => t.OnPrepareCommand(context));
    }
    public static void NotifyBeforeExecute(this AdoInterceptor interceptor, SqlExecuteContext context)
    {
        interceptor.Interceptors.ForEach(t => t.BeforeExecute(context));
    }
    public static void NotifyAfterExecute(this AdoInterceptor interceptor, SqlExecuteContext context)
    {
        interceptor.Interceptors.ForEach(t => t.AfterExecute(context));
    }
    public static void NotifyException(this AdoInterceptor interceptor, SqlExecuteExceptionContext context)
    {
        interceptor.Interceptors.ForEach(t => t.OnException(context));
    }
}
internal readonly struct AdoInterceptor(ICollection<IAdoInterceptor> interceptors)
{
    public ICollection<IAdoInterceptor> Interceptors { get; } = interceptors;

}
