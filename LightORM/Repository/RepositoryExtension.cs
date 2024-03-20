namespace LightORM.Repository;

public static class RepositoryExtension
{
    public static IRepository<T> Repository<T>(this IExpressionContext sql)
    {
        return new RepositoryImpl<T>(sql);
    }
}
