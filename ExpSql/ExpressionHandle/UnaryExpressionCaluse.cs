using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DExpSql.ExpressionHandle {
    class UnaryExpressionCaluse : BaseExpressionSql<UnaryExpression> {
        protected override SqlCaluse Where(UnaryExpression exp, SqlCaluse sqlCaluse) {
            ExpressionVisit.Where(exp.Operand, sqlCaluse);
            return sqlCaluse;
        }

        protected override SqlCaluse Select(UnaryExpression exp, SqlCaluse sqlCaluse) {
            ExpressionVisit.Select(exp.Operand, sqlCaluse);
            return sqlCaluse;
        }

        protected override SqlCaluse SelectMethod(UnaryExpression exp, SqlCaluse sqlCaluse) {
            ExpressionVisit.SelectMethod(exp.Operand, sqlCaluse);
            return sqlCaluse;
        }

        protected override SqlCaluse Join(UnaryExpression exp, SqlCaluse sqlCaluse) {
            ExpressionVisit.Join(exp.Operand, sqlCaluse);
            return sqlCaluse;
        }

        protected override SqlCaluse OrderBy(UnaryExpression exp, SqlCaluse sqlCaluse) {
            ExpressionVisit.OrderBy(exp.Operand, sqlCaluse);
            return sqlCaluse;
        }

        protected override SqlCaluse GroupBy(UnaryExpression exp, SqlCaluse sqlCaluse) {
            ExpressionVisit.GroupBy(exp.Operand, sqlCaluse);
            return sqlCaluse;
        }

        protected override SqlCaluse Update(UnaryExpression exp, SqlCaluse sqlCaluse) {
            ExpressionVisit.Update(exp.Operand, sqlCaluse);
            return sqlCaluse;
        }

        protected override SqlCaluse Insert(UnaryExpression exp, SqlCaluse sqlCaluse) {
            ExpressionVisit.Insert(exp.Operand, sqlCaluse);
            return sqlCaluse;
        }

        protected override SqlCaluse In(UnaryExpression exp, SqlCaluse sqlCaluse) {
            ExpressionVisit.In(exp.Operand, sqlCaluse);
            return sqlCaluse;
        }

        protected override SqlCaluse PrimaryKey(UnaryExpression exp, SqlCaluse sqlCaluse) {
            ExpressionVisit.PrimaryKey(exp.Operand, sqlCaluse);
            return sqlCaluse;
        }

        protected override SqlCaluse Max(UnaryExpression exp, SqlCaluse sqlCaluse) {
            ExpressionVisit.Max(exp.Operand, sqlCaluse);
            return sqlCaluse;
        }
    }
}
