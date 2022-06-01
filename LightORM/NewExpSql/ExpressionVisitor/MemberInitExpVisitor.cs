using System.Linq.Expressions;

namespace MDbContext.NewExpSql.ExpressionVisitor
{
    internal class MemberInitExpVisitor : BaseVisitor<MemberInitExpression>
    {
        public override void DoVisit(MemberInitExpression exp, SqlConfig config, SqlContext context)
        {

        }
    }
}
