using System.Linq.Expressions;

namespace MDbContext.ExpressionSql.ExpressionVisitor;

internal class ConstantExpVisitor : BaseVisitor<ConstantExpression>
{
    public override void DoVisit(ConstantExpression exp, SqlConfig config, SqlContext context)
    {
        var value = exp.Value;
        if (value == null)
        {
            context.Append(" null");
        }
        else
        {
            context.AppendDbParameter(value);
        }
    }
}
