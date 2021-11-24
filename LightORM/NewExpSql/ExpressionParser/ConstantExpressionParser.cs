using MDbContext.NewExpSql.SqlFragment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql.ExpressionParser
{
    internal class ConstantExpressionParser : BaseParser<ConstantExpression>
    {
        public override BaseFragment Where(ConstantExpression exp, WhereFragment fragment)
        {
            string pName = fragment.AddDbParameter(exp.Value);
            fragment.SqlAppend(pName);
            return fragment;
        }
    }
}
