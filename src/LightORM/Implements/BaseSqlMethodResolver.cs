using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Implements
{
    public abstract class BaseSqlMethodResolver : ISqlMethodResolver
    {
        private readonly Dictionary<string, Action<IExpressionResolver, MethodCallExpression>> methods = [];
        protected BaseSqlMethodResolver()
        {
            methods.Add(nameof(Count), Count);
            methods.Add(nameof(Sum), Sum);
            methods.Add(nameof(Avg), Avg);
            methods.Add(nameof(Max), Max);
            methods.Add(nameof(Min), Min);
            methods.Add(nameof(StartsWith), StartsWith);
            methods.Add(nameof(Contains), Contains);
            methods.Add(nameof(EndsWith), EndsWith);
            methods.Add(nameof(Substring), Substring);
            methods.Add(nameof(Trim), Trim);
            methods.Add(nameof(TrimStart), TrimStart);
            methods.Add(nameof(TrimEnd), TrimEnd);
            methods.Add(nameof(Where), Where);
            methods.Add(nameof(When), When);
            methods.Add(nameof(RowNumber), RowNumber);
            methods.Add(nameof(Lag), Lag);
            methods.Add(nameof(PartitionBy), PartitionBy);
            methods.Add(nameof(OrderBy), OrderBy);
            methods.Add(nameof(Value), Value);
        }
        public void Resolve(IExpressionResolver resolver, MethodCallExpression expression)
        {
            var methodName = expression.Method.Name;
            if (!methods.TryGetValue(methodName, out var action))
            {
                throw new NotSupportedException($"{this.GetType().Name}: {expression.Method.Name}, {expression.Method.GetParameters().Select(p => $"[{p.ParameterType}:{p.Name}]")}");
            }
            action.Invoke(resolver, expression);
        }

        public virtual void Count(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            if (methodCall.Arguments.Count > 0)
            {
                var useCaseWhen = methodCall.Method.GetParameters()[0].ParameterType == typeof(bool)
                    && methodCall.Arguments[0] is BinaryExpression;
                if (useCaseWhen)
                {
                    resolver.Sql.Append("COUNT(CASE WHEN ");
                    resolver.Visit(methodCall.Arguments[0]);
                    resolver.Sql.Append(" THEN 1 ElSE null END)");
                }
                else
                {
                    resolver.Sql.Append("COUNT(");
                    resolver.Visit(methodCall.Arguments[0]);
                    resolver.Sql.Append(')');
                }
            }
            else
            {
                resolver.Sql.Append("COUNT(*)");
            }
        }

        public virtual void Sum(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            if (methodCall.Arguments.Count > 1)
            {
                resolver.Sql.Append("SUM(CASE WHEN ");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(" THEN ");
                resolver.Visit(methodCall.Arguments[1]);
                resolver.Sql.Append(" ElSE 0 END)");
            }
            else
            {
                resolver.Sql.Append("SUM(");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(')');
            }
        }

        public virtual void Avg(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            if (methodCall.Arguments.Count > 1)
            {
                resolver.Sql.Append("AVG(CASE WHEN ");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(" THEN ");
                resolver.Visit(methodCall.Arguments[1]);
                resolver.Sql.Append(" ElSE 0 END)");
            }
            else
            {
                resolver.Sql.Append("AVG(");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(')');
            }
        }

        public virtual void Max(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            if (methodCall.Arguments.Count > 1)
            {
                resolver.Sql.Append("MAX(CASE WHEN ");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(" THEN ");
                resolver.Visit(methodCall.Arguments[1]);
                resolver.Sql.Append(" ElSE 0 END)");
            }
            else
            {
                resolver.Sql.Append("MAX(");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(')');
            }
        }

        public virtual void Min(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            if (methodCall.Arguments.Count > 1)
            {
                resolver.Sql.Append("MIN(CASE WHEN ");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(" THEN ");
                resolver.Visit(methodCall.Arguments[1]);
                resolver.Sql.Append(" ElSE 0 END)");
            }
            else
            {
                resolver.Sql.Append("MIN(");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(')');
            }
        }

        public virtual void StartsWith(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            throw new NotSupportedException();
        }

        public virtual void Contains(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            throw new NotSupportedException();
        }

        public virtual void EndsWith(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            throw new NotSupportedException();
        }

        public virtual void Substring(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            throw new NotSupportedException();
        }

        public virtual void Trim(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            throw new NotSupportedException();
        }

        public virtual void TrimEnd(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            throw new NotSupportedException();
        }

        public virtual void TrimStart(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            throw new NotSupportedException();
        }

        #region include用到的方法
        public void Where(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            if (resolver.NavigateDeep > 0)
            {
                resolver.Sql.Clear();
            }
            resolver.NavigateDeep++;
            resolver.Visit(methodCall.Arguments[0]);
            resolver.Sql.Clear();
            resolver.Visit(methodCall.Arguments[1]);
        }

        public void When(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            if (resolver.NavigateDeep > 0)
            {
                resolver.Sql.Clear();
            }
            resolver.NavigateDeep++;
            resolver.Visit(methodCall.Arguments[0]);
            resolver.Visit(methodCall.Arguments[1]);
        }
        #endregion

        #region 窗口函数

        public virtual void RowNumber(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Sql.Append("ROW_NUMBER() OVER(");
        }

        public virtual void Lag(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Sql.Append("LAG(");
            resolver.Visit(methodCall.Arguments[0]);
            resolver.Sql.Append(") OVER(");
        }




        public virtual void PartitionBy(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(" PARTITION BY ");
            resolver.Visit(methodCall.Arguments[0]);
        }

        public virtual void OrderBy(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(" ORDER BY ");
            resolver.Visit(methodCall.Arguments[0]);
        }

        public virtual void Value(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(" )");
        }


        #endregion
    }
}
