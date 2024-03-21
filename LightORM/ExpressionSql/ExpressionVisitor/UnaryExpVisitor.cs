using System.Linq.Expressions;

namespace LightORM.ExpressionSql.ExpressionVisitor;

internal class UnaryExpVisitor : BaseVisitor<UnaryExpression>
{
    public override void DoVisit(UnaryExpression exp, SqlResolveOptions config, SqlContext context)
    {
        ExpressionVisit.Visit(exp.Operand, config, context);
    }
}
