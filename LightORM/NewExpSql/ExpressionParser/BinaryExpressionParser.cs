using MDbContext.NewExpSql.SqlFragment;
using System;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql.ExpressionParser {
    internal class BinaryExpressionParser : BaseParser<BinaryExpression> {
        private void OperatorParser(ExpressionType expressionNodeType, int operatorIndex, StringBuilder content, bool useIs = false, bool newLine = true) {
            var n = newLine ? "\n" : "";
            switch (expressionNodeType) {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    content.Insert(operatorIndex, $"{n} AND");
                    break;
                case ExpressionType.Equal:
                    if (useIs) {
                        content.Insert(operatorIndex, " IS");
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
                        content.Insert(operatorIndex, " IS NOT");
                    } else {
                        content.Insert(operatorIndex, " <> ");
                    }
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    content.Insert(operatorIndex, $"{n} OR");
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


        public override BaseFragment Where(BinaryExpression exp, ISqlContext context, WhereFragment fragment) {
            context.Position = Position.Left;
            ExpressionVisit.Where(exp.Left, context,fragment);
            var insertIndex = fragment.Length;
            context.Position = Position.Right;
            ExpressionVisit.Where(exp.Right, context, fragment);
            var endIndex = fragment.Length;
            var b = endIndex - insertIndex == 5 && fragment.EndWith("null");
            OperatorParser(exp.NodeType, insertIndex, fragment.Sql, b);
            return fragment;
        }


        public override BaseFragment Join(BinaryExpression exp, ISqlContext context, JoinFragment fragment) {
            fragment.Add(" ON ");
            ExpressionVisit.Join(exp.Left, context, fragment);
            var insertIndex = fragment.Length;
            ExpressionVisit.Join(exp.Right, context, fragment);
            OperatorParser(exp.NodeType, insertIndex, fragment.Sql, false);
            return fragment;
        }


    }
}
