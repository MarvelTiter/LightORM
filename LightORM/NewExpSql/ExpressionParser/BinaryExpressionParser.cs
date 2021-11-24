using MDbContext.NewExpSql.SqlFragment;
using System;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql.ExpressionParser
{
    internal class BinaryExpressionParser : BaseParser<BinaryExpression>
    {
        private void OperatorParser(ExpressionType expressionNodeType, int operatorIndex, StringBuilder content, bool useIs = false, bool newLine = true)
        {
            var n = newLine ? "\n" : "";
            switch (expressionNodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    content.Insert(operatorIndex, $"{n} AND");
                    break;
                case ExpressionType.Equal:
                    if (useIs)
                    {
                        content.Insert(operatorIndex, " IS");
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
                        content.Insert(operatorIndex, " IS NOT");
                    }
                    else
                    {
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


        public override BaseFragment Where(BinaryExpression exp, WhereFragment fragment)
        {
            ExpressionVisit.Where(exp.Left, fragment);
            var insertIndex = fragment.Length;
            ExpressionVisit.Where(exp.Right, fragment);
            var endIndex = fragment.Length;
            var b = endIndex - insertIndex == 5 && fragment.EndWith("null");
            OperatorParser(exp.NodeType, insertIndex, fragment.Sql, b);
            return fragment;
        }

        public override BaseFragment Join(BinaryExpression exp, JoinFragment fragment)
        {
            fragment.SqlAppend(" ON ");
            ExpressionVisit.Join(exp.Left, fragment);
            var insertIndex = fragment.Length;
            ExpressionVisit.Join(exp.Right, fragment);
            OperatorParser(exp.NodeType, insertIndex, fragment.Sql, false);
            return fragment;
        }


    }
}
