using MDbContext.NewExpSql.SqlFragment;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.ExpressionParamParser
{
    class MemberParamParser : BaseParser<MemberExpression>
    {
        public override BaseFragment Where(MemberExpression exp, WhereFragment fragment)
        {
            var v = Expression.Lambda(exp).Compile().DynamicInvoke();
            fragment.AddDbParameter(v, exp.Member.Name);
            return fragment;
        }
    }
}
