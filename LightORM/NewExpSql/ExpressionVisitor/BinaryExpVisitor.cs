using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql.ExpressionVisitor
{
    internal class BinaryExpVisitor : BaseVisitor<BinaryExpression>
    {
        private void OperatorParser(ExpressionType expressionNodeType, int operatorIndex, ISqlContext content, bool useIs = false, bool newLine = true)
        {
            var n = newLine ? "\n" : "";
            switch (expressionNodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    content.Insert(operatorIndex, $"{n} AND ");
                    break;
                case ExpressionType.Equal:
                    if (useIs)
                    {
                        content.Insert(operatorIndex, " IS ");
                    }
                    else
                    {
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
                    if (useIs)
                    {
                        content.Insert(operatorIndex, " IS NOT ");
                    }
                    else
                    {
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
        public override void DoVisit(BinaryExpression exp, SqlConfig config, ISqlContext context)
        {
            config.BinaryPosition = BinaryPosition.Left;
            ExpressionVisit.Visit(exp.Left, config, context);
            var insertIndex = context.Length;

            config.BinaryPosition = BinaryPosition.Right;
            ExpressionVisit.Visit(exp.Right, config, context);

            var endIndex = context.Length;
            var b = endIndex - insertIndex == 5 && context.EndWith("null");
            OperatorParser(exp.NodeType, insertIndex, context, b);
        }
    }
}
