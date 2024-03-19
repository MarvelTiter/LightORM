﻿using LightORM.Cache;
using LightORM.Extension;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections;
using LightORM.SqlMethodResolver;
namespace LightORM.Utils;

internal static class ExpressionExtensions
{
    public static ExpressionResolvedResult Resolve(this Expression? expression, SqlResolveOptions options)
    {
        var resolve = new ExpressionResolver(options);
        resolve.Visit(expression);
        return new ExpressionResolvedResult
        {
            SqlString = resolve.Sql.ToString(),
            DbParameters = resolve.DbParameters,
        };
    }

    public static string OperatorParser(this ExpressionType expressionNodeType, bool useIs)
    {
        return expressionNodeType switch
        {
            ExpressionType.And or
            ExpressionType.AndAlso => " AND ",
            ExpressionType.Equal => useIs ? " IS " : " = ",
            ExpressionType.GreaterThan => " > ",
            ExpressionType.GreaterThanOrEqual => " >= ",
            ExpressionType.NotEqual => useIs ? " IS NOT " : " <> ",
            ExpressionType.Or or
            ExpressionType.OrElse => " OR ",
            ExpressionType.LessThan => " < ",
            ExpressionType.LessThanOrEqual => " <= ",
            _ => throw new NotImplementedException("未实现的节点类型" + expressionNodeType)
        };
    }
}

public interface IExpressionResolver
{
    bool IsNot { get; }
    StringBuilder Sql { get; }
    SqlResolveOptions Options { get; }
    Expression? Visit(Expression? expression);
}

public class ExpressionResolver(SqlResolveOptions options) : IExpressionResolver
{
    public SqlResolveOptions Options { get; } = options;
    public Dictionary<string, object> DbParameters { get; set; } = [];
    public StringBuilder Sql { get; set; } = new StringBuilder();
    public Stack<MemberInfo> Members { get; set; } = [];
    public bool IsNot { get; set; }
    public SqlMethod MethodResolver { get; } = options.DbType.GetSqlMethodResolver();
    public Expression? Visit(Expression? expression)
    {
        System.Diagnostics.Debug.WriteLine($"Current Expression: {expression}");
        return expression switch
        {
            LambdaExpression => Visit(VisitLambda((LambdaExpression)expression)),
            BinaryExpression => Visit(VisitBinary((BinaryExpression)expression)),
            MethodCallExpression => Visit(VisitMethodCall((MethodCallExpression)expression)),
            NewArrayExpression => Visit(VisitNewArray((NewArrayExpression)expression)),
            NewExpression => Visit(VisitNew((NewExpression)expression)),
            UnaryExpression => Visit(VisitUnary((UnaryExpression)expression)),
            ParameterExpression => Visit(VisitParameter((ParameterExpression)expression)),
            MemberInitExpression => Visit(VisitMemberInit((MemberInitExpression)expression)),
            MemberExpression => Visit(VisitMember((MemberExpression)expression)),
            ConstantExpression => Visit(VisitConstant((ConstantExpression)expression)),
            _ => null
        };
    }
    Expression? bodyExpression;
    Expression? VisitLambda(LambdaExpression exp)
    {
        bodyExpression = exp.Body;

        return bodyExpression;
    }

    Expression? VisitBinary(BinaryExpression exp)
    {
        // 数组访问
        if (exp.NodeType == ExpressionType.ArrayIndex)
        {
            var index = Convert.ToInt32(Expression.Lambda(exp.Right).Compile().DynamicInvoke());
            var array = Expression.Lambda(exp.Left).Compile().DynamicInvoke() as Array;
            var parameterName = $"Const_{Options.ParameterIndex}";
            AddDbParameter(parameterName, array!.GetValue(index)!);
            Sql.Append(Options.DbType.AttachPrefix(parameterName));
            return null;
        }
        if (Options.SqlType == SqlPartial.Where || Options.SqlType == SqlPartial.Join)
        {
            Sql.Append("( ");
        }

        Visit(exp.Left);
        var insertIndex = Sql.Length;
        Visit(exp.Right);
        var endIndex = Sql.Length;
        var useIs = endIndex - insertIndex == 4 && Sql.ToString(insertIndex, 4) == "NULL";
        var op = exp.NodeType.OperatorParser(useIs);
        Sql.Insert(insertIndex, op);

        if (Options.SqlType == SqlPartial.Where || Options.SqlType == SqlPartial.Join)
        {
            Sql.Append(" )");
        }

        return null;
    }



    Expression? VisitMethodCall(MethodCallExpression exp)
    {
        Members.Clear();
        if (exp.Method.Name.Equals("get_Item") && (exp.Method.DeclaringType?.FullName?.StartsWith("System.Collections.Generic") ?? false))
        {

        }
        else
        {
            MethodResolver.Invoke(this, exp);
        }
        return null;
    }

    Expression? VisitNewArray(NewArrayExpression exp)
    {
        for (int i = 0; i < exp.Expressions.Count; i++)
        {
            Visit(exp.Expressions[i]);
            if (i < exp.Expressions.Count - 1)
                Sql.Append(", ");
        }
        return null;
    }

    Expression? VisitNew(NewExpression exp)
    {
        for (int i = 0; i < exp.Arguments.Count; i++)
        {
            if (Options.SqlType == SqlPartial.Select)
            {
                Visit(exp.Arguments[i]);
                if (!NotAs)
                    Sql.Append($" AS {Options.DbType.AttachEmphasis(exp.Members![i].Name)}");
            }
            else if (Options.SqlType == SqlPartial.Insert)
            {
                var col = TableContext.GetTableInfo(exp.Type).Columns.First(c => c.Property.Name == exp.Members![i].Name);
                Sql.Append($"{Options.DbType.AttachEmphasis(col.ColumnName)} = ");
                Visit(exp.Arguments[i]);
            }
            else
            {
                Visit(exp.Arguments[i]);
            }
            if (i + 1 < exp.Arguments.Count)
            {
                Sql.Append(", \n");
            }
        }
        return null;
    }

    Expression? VisitUnary(UnaryExpression exp)
    {
        IsNot = exp.NodeType == ExpressionType.Not;
        Visit(exp.Operand);
        return null;
    }
    bool notAs;
    bool NotAs
    {
        get
        {
            if (notAs)
            {
                notAs = false;
                return true;
            }
            return notAs;
        }
        set => notAs = value;
    }
    Expression? VisitParameter(ParameterExpression exp)
    {
        var alias = TableContext.GetTableInfo(exp.Type).Alias;
        Sql.Append($"{Options.DbType.AttachEmphasis(alias!)}.*");
        NotAs = true;
        return null;
    }

    Expression? VisitMemberInit(MemberInitExpression exp)
    {
        for (int i = 0; i < exp.Bindings.Count; i++)
        {
            if (exp.Bindings[i].BindingType != MemberBindingType.Assignment)
            {
                continue;
            }
            var memberAssign = exp.Bindings[i] as MemberAssignment;
            if (Options.SqlType == SqlPartial.Select)
            {

            }
        }
        return null;
    }

    Expression? VisitMember(MemberExpression exp)
    {
        if (exp.Expression?.NodeType == ExpressionType.Parameter)
        {
            var type = exp.Type;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }
            var col = TableContext.GetTableInfo(exp.Member!.DeclaringType!).Columns.First(c => c.Property.Name == exp.Member.Name);
            Sql.Append($"{Options.DbType.AttachEmphasis(col.Table.Alias!)}.{Options.DbType.AttachEmphasis(col.ColumnName)}");
            return null;
        }
        Members.Push(exp.Member);
        return exp.Expression ?? Expression.Constant(exp.Type.TypeDefaultValue(), exp.Type);
    }

    Expression? VisitConstant(ConstantExpression exp)
    {
        var value = exp.Value;
        if (Members.Count > 0 && value != null)
        {
            value = GetValue(Members, value, out var name);
            if (value == null)
            {
                Sql.Append("NULL");
                return null;
            }
            if (value is IList list)
            {
#if NET48_OR_GREATER
                if (value.GetType().IsArray)
#else
                if (value.GetType().IsVariableBoundArray)
#endif
                {
                    return Expression.Constant(value);
                }
                var names = new List<string>();
                for (int i = 0; i < list.Count; i++)
                {
                    var n = $"{name}_{i}_{Options.ParameterIndex}";
                    names.Add(n);
                    AddDbParameter(n, list[i]!);
                }
                Sql.Append(string.Join(",", names.Select(s => $"{Options.DbType.AttachPrefix(s)}")));
            }
            else
            {
                AddDbParameter(name, value);
                Sql.Append($"{Options.DbType.AttachPrefix(name)}");
            }
        }
        else
        {
            if (value == null)
            {
                Sql.Append("NULL");
                return null;
            }
            if (exp.Type == typeof(bool) || exp.Type == typeof(DateTime))
            {
                var name = $"Const_{Options.ParameterIndex}";
                Sql.Append($"{Options.DbType.AttachPrefix(name)}");
                AddDbParameter(name, value);
            }
            else
            {
                if (exp.Type.IsNumber())
                {
                    Sql.Append($"{value}");
                }
                else
                {
                    Sql.Append($"'{value}'");
                }
            }
        }
        return null;
    }

    private void AddDbParameter(string parameterName, object v)
    {
        DbParameters.Add(parameterName, v);
        Options.ParameterIndex++;
    }

    /// <summary>
    /// 获取值
    /// </summary>
    /// <param name="memberInfos">成员信息</param>
    /// <param name="compilerVar">编译器变量值</param>
    /// <param name="memberName">成员名称</param>
    /// <returns></returns>
    public static object GetValue(Stack<MemberInfo> memberInfos, object compilerVar, out string memberName)
    {
        var names = new List<string>();
        while (memberInfos.Count > 0)
        {
            var item = memberInfos.Pop();
            if (!item.Name.StartsWith("CS$<>8__locals"))
            {
                names.Add(item.Name);
            }

            compilerVar = GetValue(item, compilerVar);
        }
        memberName = string.Join("_", names);
        return compilerVar;
    }

    /// <summary>
    /// 获取值
    /// </summary>
    /// <param name="memberInfo">成员信息</param>
    /// <param name="obj">对象</param>
    /// <returns></returns>
    public static object GetValue(MemberInfo memberInfo, object obj)
    {
        if (obj == null)
        {
            return null;
        }
        if (memberInfo.MemberType == MemberTypes.Property)
        {
            var propertyInfo = memberInfo as PropertyInfo;
            return propertyInfo!.GetValue(obj)!;
        }
        else if (memberInfo.MemberType == MemberTypes.Field)
        {
            var fieldInfo = memberInfo as FieldInfo;
            return fieldInfo!.GetValue(obj)!;
        }
        return new NotSupportedException($"不支持获取 {memberInfo.MemberType} 类型值.");
    }
}