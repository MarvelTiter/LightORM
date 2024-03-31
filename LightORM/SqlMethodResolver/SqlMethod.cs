using LightORM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.SqlMethodResolver
{
    public abstract class SqlMethod
    {
        protected readonly Dictionary<string, Action<IExpressionResolver, MethodCallExpression>> methods = [];
        internal virtual void Invoke(ExpressionResolver resolver, MethodCallExpression expression)
        {
            if (!methods.TryGetValue(expression.Method.Name, out var action))
            {
                throw new NotSupportedException($"{resolver.Options.DbType}: {expression.Method.Name}");
            }
            action.Invoke(resolver, expression);
        }
        public virtual void AddAdditionMethod(string methodName, Action<ExpressionResolver, MethodCallExpression> action)
        {

        }
    }
}
