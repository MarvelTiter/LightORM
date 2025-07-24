using LightORM.Extension;
using Microsoft.Extensions.Options;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace LightORM.Utils;

internal class ExpressionValueExtract : ExpressionVisitor
{
    private readonly List<DbParameterInfo> parameters = [];
    private int index = 0;
    private readonly Stack<MemberInfo> members = [];
    public static ExpressionValueExtract Default => new();
    private bool isVisitArray = false;
    private int visitArrayIndex = -1;
    private ResolveContext? context;
    private SqlResolveOptions? option;
    public List<DbParameterInfo> Extract(Expression? exp)
    {
        if (exp is not null)
        {
            Visit(exp);
        }
        return parameters;
    }
    public List<DbParameterInfo> Extract(Expression? exp, SqlResolveOptions option, ResolveContext context)
    {
        if (exp is not null)
        {
            this.context = context;
            this.option = option;
            Visit(exp);
        }
        return parameters;
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        if (node.NodeType == ExpressionType.ArrayIndex)
        {
            var index = Convert.ToInt32(Expression.Lambda(node.Right).Compile().DynamicInvoke());
            isVisitArray = true;
            visitArrayIndex = index;
        }
        return base.VisitBinary(node);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression?.NodeType == ExpressionType.Parameter)
        {
            members.Clear();
        }
        else
        {
            members.Push(node.Member);
        }
        return base.VisitMember(node);
    }


    protected override Expression VisitConstant(ConstantExpression node)
    {
        var value = node.Value;
        if (members.Count > 0 && value != null)
        {
            value = ExpressionResolver.GetValue(members, value, out var name);
            var bn = ExpressionResolver.FormatDbParameterName(context, option, name, ref index);
            VariableValue(value, bn);
        }

        return node;

        void VariableValue(object? v, string bn)
        {
            if (v == null)
            {
                parameters.Add(new DbParameterInfo(bn, null, ExpValueType.Null));
                return;
            }

            if (isVisitArray && v is Array arr)
            {
                var value = arr.GetValue(visitArrayIndex);
                var pn = ExpressionResolver.FormatDbParameterName(context, option, $"Arr{visitArrayIndex}", ref index);
                parameters.Add(new(pn, value, ExpValueType.Other));
            }
            else if (v is IEnumerable && v is not string)
            {
                parameters.Add(new(bn, v, ExpValueType.Collection));
            }
            else if (v is bool)
            {
                parameters.Add(new(bn, v, ExpValueType.Boolean));
            }
            else
            {
                parameters.Add(new(bn, v, ExpValueType.Other));
            }
        }
    }
}
