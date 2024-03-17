using MDbContext.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace LightORM.Repository;

public static class RepositoryExtension
{
    public static IRepository<T> Repository<T>(this IExpressionContext sql)
    {
        //return new RepositoryImpl<T>(sql);
        throw new NotImplementedException();
    }
}
