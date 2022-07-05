using MDbContext.ExpressionSql;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDbContext.Repository
{
    public static class RepositoryExtension
    {
        public static IRepository<T> Repository<T>(this IExpressionContext sql)
        {
            return new RepositoryImpl<T>(sql);
        }
    }
}
