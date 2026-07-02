using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace LightORM.Extension;

public static class MethodCallExtensions
{
    public static bool IsWindowFn(this MethodCallExpression? methodCall)
    {
        return methodCall?.Method?.DeclaringType?.Name.StartsWith("IWindowFunction") == true;
    }

    public static bool IsExpSelect(this MethodCallExpression? methodCall)
    {
        return methodCall?.Method?.DeclaringType?.Name?.StartsWith("IExpSelect") == true;
    }

    public static bool IsExpSelectGrouping(this MethodCallExpression methodCall)
    {
        return methodCall?.Method?.DeclaringType?.Name?.StartsWith("IExpSelectGrouping") == true;
    }

    public static bool IsLinqExtension(this MethodCallExpression methodCall)
    {
        return methodCall?.Method?.DeclaringType == typeof(Enumerable);
    }

    [Obsolete("AOT不安全，弃用")]
#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL3050", Justification = "已弃用")]
#endif
    public static IExpSelect? GetExpSelectObject(this MethodCallExpression? methodCall, ReadOnlyCollection<ParameterExpression>? parameters = null)
    {
        Expression? obj = methodCall?.Object;
        if (methodCall?.Object is null)
        {
            //静态方法
            obj = methodCall?.Arguments[0];
        }

        if (obj is null) return null;
        IExpSelect? sel;
        if (parameters is not null)
        {
            sel = GetExpSelect(obj);
            if (sel is not null)
            {
                sel.SqlBuilder.MainTable.Index = parameters.Count;
                sel.SqlBuilder.TableIndexFix = parameters.Count;
                // 处理Where, Join等方法, 加上参数
                CollectExpressions(sel, obj, parameters);
            }
        }
        else
        {
            sel = Expression.Lambda(obj).Compile().DynamicInvoke() as IExpSelect;
        }

        return sel;

        static IExpSelect? GetExpSelect(Expression? expression)
        {
            if (expression is MethodCallExpression methodCall)
            {
                var obj = methodCall.Object ?? methodCall.Arguments.FirstOrDefault();
                if (methodCall.Method.Name == "Select")
                {
                    var select = Expression.Lambda(methodCall).Compile().DynamicInvoke() as IExpSelect;
                    return select;
                }

                return GetExpSelect(obj);
            }

            return null;
        }

        static void CollectExpressions(IExpSelect select, Expression? expression, ReadOnlyCollection<ParameterExpression> parameters)
        {
            if (expression is MethodCallExpression methodCall)
            {
                var obj = methodCall.Object ?? methodCall.Arguments.FirstOrDefault();
                if (methodCall.Method.Name == "Where")
                {
                    var exp = methodCall.Arguments[0];
                    if (exp.TryGetLambdaExpression(out var lambdaExp))
                    {
                        var newExp = Expression.Lambda(lambdaExp!.Body, [.. parameters, .. lambdaExp.Parameters]);
                        select.WhereHandle(newExp);
                    }
                }
                else if (methodCall.Method.Name == "InnerJoin")
                {
                    var exp = methodCall.Arguments[0];
                    if (exp.TryGetLambdaExpression(out var lambdaExp))
                    {
                        var newExp = Expression.Lambda(lambdaExp!.Body, [.. parameters, .. lambdaExp.Parameters]);
                        var joinType = methodCall.Method.GetGenericArguments().FirstOrDefault() ?? throw new ArgumentException("Exits条件中Join必须使用泛型方法");
                        select.JoinHandle(joinType, newExp, TableLinkType.InnerJoin);
                    }
                }
                else if (methodCall.Method.Name == "LeftJoin")
                {
                    var exp = methodCall.Arguments[0];
                    if (exp.TryGetLambdaExpression(out var lambdaExp))
                    {
                        var newExp = Expression.Lambda(lambdaExp!.Body, [.. parameters, .. lambdaExp.Parameters]);
                        var joinType = methodCall.Method.GetGenericArguments().FirstOrDefault() ?? throw new ArgumentException("Exits条件中Join必须使用泛型方法");
                        select.JoinHandle(joinType, newExp, TableLinkType.LeftJoin);
                    }
                }
                else if (methodCall.Method.Name == "RightJoin")
                {
                    var exp = methodCall.Arguments[0];
                    if (exp.TryGetLambdaExpression(out var lambdaExp))
                    {
                        var newExp = Expression.Lambda(lambdaExp!.Body, [.. parameters, .. lambdaExp.Parameters]);
                        var joinType = methodCall.Method.GetGenericArguments().FirstOrDefault() ?? throw new ArgumentException("Exits条件中Join必须使用泛型方法");
                        select.JoinHandle(joinType, newExp, TableLinkType.RightJoin);
                    }
                }

                CollectExpressions(select, obj, parameters);
            }
        }
    }

    private class InvokeContext
    {
        public List<ExpressionInfo> MethodCall { get; set; } = [];
        public IExpSelect? Select { get; set; }
    }

    internal static SelectBuilder? CreateSelectBuilder(this MethodCallExpression? methodCall, int deep = 0)
    {
        if (methodCall is null)
        {
            return null;
        }

        var builder = SelectBuilder.GetSelectBuilder(deep);
        Stack<MethodCallExpression> chain = new();
        Expression? current = methodCall;
        while (current is MethodCallExpression m)
        {
            chain.Push(m);
            current = m.Object ?? m.Arguments.FirstOrDefault();
        }

        // 从内到外处理
        while (chain.Count > 0)
        {
            var m = chain.Pop();
            switch (m.Method.Name)
            {
                case "Select":
                    switch (m.Arguments.Count)
                    {
                        case 0:
                            {
                                // context.Select<Role>() 或 context.Select<Role>("table")
                                var tableType = m.Method.GetGenericArguments()[0];

                                builder.AddTableInfo(TableInfo.Create(tableType, 0));
                                break;
                            }
                        case > 0:
                            {
                                // 扩展方法，可能有多个泛型参数，也可能有重写表名参数
                                var tableType = m.Method.GetGenericArguments();
                                if (tableType.Length > 1)
                                {
                                    for (var index = 0; index < tableType.Length; index++)
                                    {
                                        var type = tableType[index];
                                        builder.AddTableInfo(TableInfo.Create(type, index));
                                    }
                                }
                                else if (tableType.Length == 1)
                                {
                                    // 单个泛型参数，可能存在重写表名参数
                                    if (m.Arguments.Count == 2 && ResolveHelper.TryExtractValue<string>(m.Arguments[1], out var overName))
                                    {
                                        builder.AddTableInfo(TableInfo.Create(overName, tableType[0], 0));
                                    }
                                    else
                                    {
                                        builder.AddTableInfo(TableInfo.Create(tableType[0], 0));
                                    }
                                }

                                break;
                            }
                    }

                    break;

                case "Where":
                    if (m.Arguments[0].TryGetLambdaExpression(out var wLambda))
                        builder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.Where, wLambda));
                    break;

                case "WhereIf":
                    if (m.Arguments[0] is ConstantExpression { Value: true }
                        && m.Arguments[1].TryGetLambdaExpression(out var wifLambda))
                        builder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.Where, wifLambda));
                    break;

                case "InnerJoin":
                    HandleJoin(builder, m, TableLinkType.InnerJoin);
                    break;
                case "LeftJoin":
                    HandleJoin(builder, m, TableLinkType.LeftJoin);
                    break;
                case "RightJoin":
                    HandleJoin(builder, m, TableLinkType.RightJoin);
                    break;
                case "OuterJoin":
                    HandleJoin(builder, m, TableLinkType.OuterJoin);
                    break;

                case "OrderBy":
                    if (m.Arguments[0].TryGetLambdaExpression(out var oLambda))
                        builder.Expressions.Add(new ExpressionInfo(
                            SqlResolveOptions.Order, oLambda, additionalParameter: "ASC"));
                    break;
                case "OrderByDesc":
                    if (m.Arguments[0].TryGetLambdaExpression(out var odLambda))
                        builder.Expressions.Add(new ExpressionInfo(
                            SqlResolveOptions.Order, odLambda, additionalParameter: "DESC"));
                    break;

                case "Distinct":
                    builder.IsDistinct = true;
                    break;

                case "Skip":
                    builder.Skip = GetConstantInt(m.Arguments[0]);
                    break;
                case "Take":
                    builder.Take = GetConstantInt(m.Arguments[0]);
                    break;
            }
        }

        return builder;

        static void HandleJoin(SelectBuilder builder, MethodCallExpression m, TableLinkType joinType)
        {
            if (m.Arguments.Count == 1)
            {
                if (!m.Arguments[0].TryGetLambdaExpression(out var lambda)) return;
                var joinTypeArg = m.Method.GetGenericArguments()[0];
                builder.JoinHandle(joinTypeArg, lambda, joinType);
            }
            else if (m.Arguments.Count == 2)
            {
                if (!m.Arguments[1].TryGetLambdaExpression(out var lambda)) return;
                var firstArg = m.Arguments[0];
                var joinTypeArg = m.Method.GetGenericArguments()[0];
                if (ResolveHelper.TryExtractValue<string>(firstArg, out var overName))
                {
                    builder.JoinHandle(joinTypeArg, lambda, joinType, overriddenTableName: overName);
                }
                else if (ResolveHelper.TryExtractValue<IExpSelect>(firstArg, out var select))
                {
                    builder.JoinHandle(joinTypeArg, lambda, joinType, subQuery: select);
                }
            }
        }

        static int GetConstantInt(Expression expr)
        {
            return expr switch
            {
                ConstantExpression { Value: int i } => i,
                ConstantExpression { Value: long l } => (int)l,
                _ => 0
            };
        }
    }

}

public static class ExpressionExtensions
{
    public static bool TryGetLambdaExpression(this Expression? expression, out LambdaExpression? result)
    {
        if (expression is null)
        {
            result = null;
            return false;
        }

        if (expression is LambdaExpression lambdaExpression)
        {
            result = lambdaExpression;
            return true;
        }

        if (expression is UnaryExpression { Operand: LambdaExpression l })
        {
            result = l;
            return true;
        }

        result = null;
        return false;
    }
}