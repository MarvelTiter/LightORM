using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DExpSql.ExpressionHandle
{
    class ConstantExpressionCaluse : BaseExpressionSql<ConstantExpression>
    {
        protected override SqlCaluse Where(ConstantExpression exp, SqlCaluse sqlCaluse)
        {
            sqlCaluse += sqlCaluse.AddDbParameter(exp.Value);
            return sqlCaluse;
        }

        protected override SqlCaluse SelectMethod(ConstantExpression exp, SqlCaluse sqlCaluse) {
            var p = sqlCaluse.AddDbParameter(exp.Value);
            sqlCaluse.SelectMethod.Append(p);
            return sqlCaluse;
        }

        protected override SqlCaluse In(ConstantExpression exp, SqlCaluse sqlCaluse)
        {
            sqlCaluse += $"'{exp.Value}'";
            return sqlCaluse;
        }
    }
}
