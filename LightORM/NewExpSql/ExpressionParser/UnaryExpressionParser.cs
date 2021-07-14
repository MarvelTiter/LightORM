using MDbContext.NewExpSql.SqlFragment;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.ExpressionParser {
    internal class UnaryExpressionParser : BaseParser<UnaryExpression> {
        protected override BaseFragment Select(UnaryExpression exp, ISqlContext context, SelectFragment fragment) {
            ExpressionVisit.Select(exp.Operand, context, fragment);
            return fragment;
        }
        public override BaseFragment Where(UnaryExpression exp, ISqlContext context, WhereFragment fragment) {
            ExpressionVisit.Where(exp.Operand, context, fragment);
            return fragment;
        }
    }
}
