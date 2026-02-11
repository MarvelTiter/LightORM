using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Utils;

internal class ResolveHelper
{
    public static string FormatDbParameterName(ResolveContext? context, SqlResolveOptions? option, string name, ref int index)
    {
        var p = $"{context?.ParameterPrefix}{name}_{option?.ParameterPartialIndex}_{index}";
        index += 1;
        return p;
    }
    // 尝试将表达式求值为常量（仅支持 Constant 和 简单 Member 访问）
    public static T ExtractInstanceValue<T>(Expression expression)
    {
        if (expression is ConstantExpression ce && ce.Value is T index)
        {
            return index;
        }
        var members = new Stack<MemberInfo>();
        Expression? current = expression;

        // 向下遍历，收集 MemberInfo
        while (current is MemberExpression memberExpr)
        {
            members.Push(memberExpr.Member);
            current = memberExpr.Expression;
        }
        object? value;

        if (current is ConstantExpression constExpr)
        {
            value = constExpr.Value;
        }
        else if (current is null)
        {
            value = null;
        }
        else
        {
            throw new LightOrmException($"数组索引表达式必须以常量或者Null结尾，但得到: {current?.GetType().Name}: {current}");
        }

        value = GetValue(members, value);

        if (value is T t)
        {
            return t;
        }
        throw new LightOrmException($"尝试获取类型{typeof(T)}的值，实际类型: {value?.GetType()}");
    }

    public static object? GetValue(Stack<MemberInfo> memberInfos, object? compilerVar) => GetValueByExpression(memberInfos, compilerVar, out _);

    private static readonly ConcurrentDictionary<string, Func<object?, object?>> getterCache = [];
    public static object? GetValueByExpression(Stack<MemberInfo> memberInfos, object? value, out string name)
    {
        name = string.Join("_", memberInfos.Where(m => !m.Name.StartsWith("CS$<>8__locals")).Select(m => m.Name));
        if (value is null) return null;
        var type = value.GetType();
        var memberKey = $"{type.FullName}_{name}";
        var func = getterCache.GetOrAdd(memberKey, _ =>
        {
            return CreateGetter(type, memberInfos);
        });
        memberInfos.Clear();
        return func.Invoke(value);
    }

    private static Func<object?, object?> CreateGetter(Type type, Stack<MemberInfo> memberInfos)
    {
        var param = Expression.Parameter(typeof(object), "obj");
        Expression body = Expression.Convert(param, type);
        while (memberInfos.Count > 0)
        {
            body = Expression.MakeMemberAccess(body, memberInfos.Pop());
        }
        body = Expression.Convert(body, typeof(object));
        var lambda = Expression.Lambda<Func<object?, object?>>(body, param);
        return lambda.Compile();
    }
}
