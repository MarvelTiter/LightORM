using System.Collections.Generic;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql
{
    internal interface IExpressionVisitor
    {
        void Visit(Expression expression, SqlConfig config, ISqlContext context);
    }
}
