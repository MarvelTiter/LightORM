using DExpSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpSql.ExpressionHandle {
    class LambdaExpressionCaluse : BaseExpressionSql<LambdaExpression> {
        protected override SqlCaluse SelectMethod(LambdaExpression exp, SqlCaluse sqlCaluse) {
            ExpressionVisit.SelectMethod(exp.Body, sqlCaluse);
            return sqlCaluse;
        }

        protected override SqlCaluse PrimaryKey(LambdaExpression exp, SqlCaluse sqlCaluse) {
            return base.PrimaryKey(exp, sqlCaluse);
        }
    }
}
