using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public static IExpSelect? GetExpSelectObject(this MethodCallExpression? methodCall)
    {
        Expression? obj = methodCall?.Object;
        if (methodCall?.Object is null)
        {
            //静态方法
            obj = methodCall?.Arguments[0];
        }
        if (obj is null) return null;
        var sel = Expression.Lambda(obj).Compile().DynamicInvoke() as IExpSelect;
        return sel;
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
        if (expression is  LambdaExpression lambdaExpression)
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
