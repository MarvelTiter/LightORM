using MDbContext.ExpressionSql;
using MDbContext.ExpressionSql.ExpressionVisitor;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.ExpressionVisitor
{
    internal class UnaryExpVisitor : BaseVisitor<UnaryExpression>
    {
        public override void DoVisit(UnaryExpression exp, SqlConfig config, SqlContext context)
        {
            ExpressionVisit.Visit(exp.Operand, config, context);
        }
    }
}
