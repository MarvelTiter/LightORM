using System;
using System.Linq.Expressions;

namespace MDbContext.NewExpSql.ExpressionVisitor
{
    internal class NewExpVisitor : BaseVisitor<NewExpression>
    {
        public override void DoVisit(NewExpression exp, SqlConfig config, SqlContext context)
        {
            var len = exp.Arguments.Count;
            for (int i = 0; i < len; i++)
            {
                var argExp = exp.Arguments[i];
                ExpressionVisit.Visit(argExp, config, context);
            }
        }
    }
}
