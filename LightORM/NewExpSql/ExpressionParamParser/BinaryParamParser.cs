using MDbContext.NewExpSql.SqlFragment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql.ExpressionParamParser
{
    class BinaryParamParser : BaseParser<BinaryExpression>
    {
        public override BaseFragment Where(BinaryExpression exp, WhereFragment fragment)
        {
            ExpressionParamVisit.Where(exp.Right, fragment);
            return fragment;
        }
    }
}
