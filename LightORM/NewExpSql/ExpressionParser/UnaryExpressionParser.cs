using MDbContext.NewExpSql.SqlFragment;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.ExpressionParser
{
    internal class UnaryExpressionParser : BaseParser<UnaryExpression>
    {
        protected override BaseFragment Select(UnaryExpression exp, SelectFragment fragment)
        {
            ExpressionVisit.Select(exp.Operand, fragment);
            return fragment;
        }
        public override BaseFragment Where(UnaryExpression exp, WhereFragment fragment)
        {
            ExpressionVisit.Where(exp.Operand, fragment);
            return fragment;
        }
    }
}
