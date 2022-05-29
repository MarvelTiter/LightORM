using System.Linq.Expressions;

namespace MDbContext.NewExpSql.ExpressionVisitor
{
    internal class UnaryExpVisitor : BaseVisitor<UnaryExpression>
    {
        public override void DoVisit(UnaryExpression exp, SqlConfig config, ISqlContext context)
        {
            ExpressionVisit.Visit(exp.Operand, config, context);
        }
    }
}
