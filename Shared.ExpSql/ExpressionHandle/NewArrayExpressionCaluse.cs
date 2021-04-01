using DExpSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpSql.ExpressionHandle
{
    internal class NewArrayExpressionCaluse : BaseExpressionSql<NewArrayExpression>
    {
        protected override SqlCaluse Where(NewArrayExpression exp, SqlCaluse sqlCaluse)
        {
            sqlCaluse += "(";
            foreach (var item in exp.Expressions)
            {
                ExpressionVisit.In(item, sqlCaluse);
                sqlCaluse += ",";
            }
            sqlCaluse -= ",";
            sqlCaluse += ")";
            return sqlCaluse;
        }
    }
}
