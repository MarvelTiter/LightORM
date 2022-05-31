using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.NewExpSql.ExpressionVisitor
{
    internal abstract class BaseVisitor<T> : IExpressionVisitor where T : Expression
    {
        public void Visit(Expression expression, SqlConfig config, SqlContext context)
        {
            DoVisit((T)expression, config, context);
        }

        public abstract void DoVisit(T exp, SqlConfig config, SqlContext context);
    }
}
