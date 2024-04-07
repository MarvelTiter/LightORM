using LightORM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.SqlMethodResolver;

public abstract class SqlMethod
{
    protected readonly Dictionary<string, Action<IExpressionResolver, MethodCallExpression>> methods = [];
    internal virtual void Invoke(ExpressionResolver resolver, MethodCallExpression expression)
    {
        if (!methods.TryGetValue(expression.Method.Name, out var action))
        {
            try
            {
                TryResolveExpression(resolver, expression);
            }
            catch
            {
                throw new NotSupportedException($"{resolver.Options.DbType}: {expression.Method.Name}");
            }
        }
        action.Invoke(resolver, expression);
    }

    private static void TryResolveExpression(ExpressionResolver resolver, MethodCallExpression expression)
    {
        //var 
        resolver.Visit(expression.Arguments[1]);
    }
}
