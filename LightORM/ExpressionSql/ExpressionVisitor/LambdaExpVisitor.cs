using System.Linq.Expressions;

namespace MDbContext.ExpressionSql.ExpressionVisitor;

internal class LambdaExpVisitor : BaseVisitor<LambdaExpression>
{
    public override void DoVisit(LambdaExpression exp, SqlConfig config, SqlContext context)
    {
        ExpressionVisit.Visit(exp.Body, config, context);
    }
}
