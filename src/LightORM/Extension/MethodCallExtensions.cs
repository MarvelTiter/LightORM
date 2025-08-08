using System.Collections.ObjectModel;

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
    }
    private class InvokeContext
    {
        public List<ExpressionInfo> MethodCall { get; set; } = [];
        public IExpSelect? Select { get; set; }
    }
    private static IExpSelect? GetExpSelect(Expression? expression)
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
    private static void CollectExpressions(IExpSelect select, Expression? expression, ReadOnlyCollection<ParameterExpression> parameters)
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
        if (expression is UnaryExpression u && u.Operand is LambdaExpression l)
        {
            result = l;
            return true;
        }
        result = null;
        return false;
    }
}
