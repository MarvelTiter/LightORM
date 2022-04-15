using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DExpSql.ExpressionHandle {
    class BinaryExpressionCaluse : BaseExpressionSql<BinaryExpression> {
        private void OperatorParser(ExpressionType expressionNodeType, int operatorIndex, StringBuilder content, bool useIs = false, bool newLine = true) {
            var n = newLine ? "\n" : "";
            switch (expressionNodeType) {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    content.Insert(operatorIndex, $"{n} AND ");
                    break;
                case ExpressionType.Equal:
                    if (useIs) {
                        content.Insert(operatorIndex, " IS ");
                    } else {
                        content.Insert(operatorIndex, " = ");
                    }
                    break;
                case ExpressionType.GreaterThan:
                    content.Insert(operatorIndex, " > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    content.Insert(operatorIndex, " >= ");
                    break;
                case ExpressionType.NotEqual:
                    if (useIs) {
                        content.Insert(operatorIndex, " IS NOT ");
                    } else {
                        content.Insert(operatorIndex, " <> ");
                    }
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    content.Insert(operatorIndex, $"{n} OR ");
                    break;
                case ExpressionType.LessThan:
                    content.Insert(operatorIndex, " < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    content.Insert(operatorIndex, " <= ");
                    break;
                default:
                    throw new NotImplementedException("未实现的节点类型" + expressionNodeType);
            }
        }

        protected override SqlCaluse SelectMethod(BinaryExpression exp, SqlCaluse sqlCaluse) {
            sqlCaluse.SelectMethodType = 0;
            ExpressionVisit.SelectMethod(exp.Left, sqlCaluse);
            var insertIndex = sqlCaluse.SelectMethod.Length;
            ExpressionVisit.SelectMethod(exp.Right, sqlCaluse);
            var endIndex = sqlCaluse.SelectMethod.Length;
            var b = endIndex - insertIndex == 5 && sqlCaluse.SelectMethod.ToString().EndsWith("null");
            OperatorParser(exp.NodeType, insertIndex, sqlCaluse.SelectMethod, b, false);
            return sqlCaluse;
        }

        protected override SqlCaluse Where(BinaryExpression exp, SqlCaluse sqlCaluse) {
            ExpressionVisit.Where(exp.Left, sqlCaluse);
            var insertIndex = sqlCaluse.Length;

            ExpressionVisit.Where(exp.Right, sqlCaluse);
            var endIndex = sqlCaluse.Length;
            var b = endIndex - insertIndex == 5 && sqlCaluse.EndWith("null");
            OperatorParser(exp.NodeType, insertIndex, sqlCaluse.Sql, b);
            return sqlCaluse;
        }

        protected override SqlCaluse Join(BinaryExpression exp, SqlCaluse sqlCaluse) {
            //sqlCaluse += " AND ";
            ExpressionVisit.Join(exp.Left, sqlCaluse);
            var insertIndex = sqlCaluse.Length;

            ExpressionVisit.Join(exp.Right, sqlCaluse);
            OperatorParser(exp.NodeType, insertIndex, sqlCaluse.Sql, false);

            return sqlCaluse;
        }
    }
}
