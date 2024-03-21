using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace LightORM.ExpressionSql.ExpressionVisitor;

internal class BinaryExpVisitor : BaseVisitor<BinaryExpression>
{
    private void OperatorParser(ExpressionType expressionNodeType, int operatorIndex, SqlContext content, bool useIs = false, bool newLine = false)
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

    private string OperatorParser(ExpressionType expressionNodeType, bool useIs)
    {
        return expressionNodeType switch
        {
            ExpressionType.And or
            ExpressionType.AndAlso => "AND",
            ExpressionType.Equal => useIs ? "IS" : "=",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.NotEqual => useIs ? "IS NOT" : "<>",
            ExpressionType.Or or
            ExpressionType.OrElse => "OR",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            _ => throw new NotImplementedException("未实现的节点类型" + expressionNodeType)
        };
    }

    public override void DoVisit(BinaryExpression exp, SqlResolveOptions config, SqlContext context)
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
