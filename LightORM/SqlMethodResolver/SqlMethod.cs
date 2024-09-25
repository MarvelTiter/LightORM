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
                TryResolveExpression(expression.Method.Name, resolver, expression);
                return;
            }
            catch
            {
                throw new NotSupportedException($"{resolver.Options.DbType}: {expression.Method.Name}, {expression.Method.GetParameters().Select(p => $"[{p.ParameterType}:{p.Name}]")}");
            }
        }
        action.Invoke(resolver, expression);
    }

    private static void TryResolveExpression(string methodName, ExpressionResolver resolver, MethodCallExpression expression)
    {
        if (resolver.NavigateDeep > 0)
        {
            resolver.Sql.Clear();
        }
        if (methodName == nameof(Enumerable.Where))
        {
            HandleIncludeWhere(resolver, expression);
            return;
        }
        else if (methodName == nameof(IncludeExtensions.When))
        {
            resolver.NavigateDeep++;
            resolver.Visit(expression.Arguments[0]);
            resolver.Visit(expression.Arguments[1]);
        }
    }

    private static void HandleIncludeWhere(ExpressionResolver resolver, MethodCallExpression expression)
    {
        resolver.NavigateDeep++;
        resolver.Visit(expression.Arguments[0]);
        resolver.Sql.Clear();
        resolver.Visit(expression.Arguments[1]);
    }
}
