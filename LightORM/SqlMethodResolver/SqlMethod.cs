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
                return;
            }
            catch
            {
                throw new NotSupportedException($"{resolver.Options.DbType}: {expression.Method.Name}, {expression.Method.GetParameters().Select(p => $"[{p.ParameterType}:{p.Name}]")}");
            }
        }
        action.Invoke(resolver, expression);
    }

    private static void TryResolveExpression(ExpressionResolver resolver, MethodCallExpression expression)
    {
        if (resolver.NavigateDeep > 0)
        {
            resolver.Sql.Clear();
        }
        resolver.NavigateDeep++;
        resolver.Visit(expression.Arguments[0]);
        resolver.Visit(expression.Arguments[1]);
    }
}
