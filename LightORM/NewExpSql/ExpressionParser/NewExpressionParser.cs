using MDbContext.NewExpSql.SqlFragment;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.ExpressionParser {
    internal class NewExpressionParser : BaseParser<NewExpression> {
        protected override BaseFragment Select(NewExpression exp, ISqlContext context, SelectFragment fragment) {
            for (int i = 0; i < exp.Arguments.Count; i++) {
                var argExp = exp.Arguments[i];                
                ExpressionVisit.Select(argExp, context,fragment);
            }
            return fragment;
        }
    }
}
