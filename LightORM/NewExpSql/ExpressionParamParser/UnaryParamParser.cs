using MDbContext.NewExpSql.SqlFragment;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.ExpressionParamParser
{
    class UnaryParamParser : BaseParser<UnaryExpression>
    {
        public override BaseFragment Where(UnaryExpression exp, WhereFragment fragment)
        {
            ExpressionParamVisit.Where(exp.Operand, fragment);
            return fragment;
        }
    }
}
