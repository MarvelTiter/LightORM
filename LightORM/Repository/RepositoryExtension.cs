using LightORM.Repository;

namespace LightORM;

public static class RepositoryExtension
{
    public static IRepository<T> Repository<T>(this IExpressionContext sql)
    {
        return new RepositoryImpl<T>(sql);
    }
}
