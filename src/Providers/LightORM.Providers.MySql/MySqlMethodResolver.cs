using LightORM.Extension;
using LightORM.Implements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.MySql;

public sealed class MySqlMethodResolver : BaseSqlMethodResolver
{
    public override void StartsWith(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        resolver.Visit(methodCall.Object);
        resolver.Sql.Append(resolver.IsNot ? " NOT LIKE " : " LIKE ");
        resolver.Sql.Append("CONCAT(");
        resolver.Visit(methodCall.Arguments[0]);
        resolver.Sql.Append(", '%')");
    }

    public override void Contains(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        if (methodCall.Object != null && (methodCall.Object.Type.FullName?.StartsWith("System.Collections.Generic") ?? false))
        {
            resolver.Visit(methodCall.Arguments[0]);
            resolver.Sql.Append(resolver.IsNot ? " NOT IN " : " IN ");
            resolver.Sql.Append('(');
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(')');
        }
        else if (methodCall.Method.DeclaringType == typeof(Enumerable))
        {
            resolver.Visit(methodCall.Arguments[1]);
            resolver.Sql.Append(resolver.IsNot ? " NOT IN " : " IN ");
            resolver.Sql.Append('(');
            resolver.Visit(methodCall.Arguments[0]);
            resolver.Sql.Append(')');
        }
        else
        {
            // 字符串
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(resolver.IsNot ? " NOT LIKE " : " LIKE ");
            resolver.Sql.Append("CONCAT('%', ");
            resolver.Visit(methodCall.Arguments[0]);
            resolver.Sql.Append(", '%')");
        }
    }

    public override void EndsWith(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        resolver.Visit(methodCall.Object);
        resolver.Sql.Append(resolver.IsNot ? " NOT LIKE " : " LIKE ");
        resolver.Sql.Append("CONCAT('%', ");
        resolver.Visit(methodCall.Arguments[0]);
        resolver.Sql.Append(')');
    }

    public override void Substring(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        // SUBSTRING(columnName, startIndex, endIndex?);
        resolver.Sql.Append("SUBSTR");
        resolver.Sql.Append('(');
        resolver.Visit(methodCall.Object);
        resolver.Sql.Append(',');
        resolver.Visit(methodCall.Arguments[0]);
        if (methodCall.Arguments.Count > 1)
        {
            resolver.Sql.Append(',');
            resolver.Visit(methodCall.Arguments[1]);
        }
        resolver.Sql.Append(')');
    }

    public override void OrderBy(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        if (methodCall.IsWindowFn())
        {
            base.OrderBy(resolver, methodCall);
        }
        else
        {
            resolver.Visit(methodCall.Object);
            resolver.ExpStores!.Add("OrderBy", methodCall.Arguments[0]);
        }
    }

    public override void OrderByDesc(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        if (methodCall.IsWindowFn())
        {
            base.OrderByDesc(resolver, methodCall);
        }
        else
        {
            resolver.Visit(methodCall.Object);
            resolver.ExpStores!.Add("OrderByDesc", methodCall.Arguments[0]);
        }
    }

    public override void Value(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        if (methodCall.IsWindowFn())
        {
            base.Value(resolver, methodCall);
        }
        else
        {
            resolver.Visit(methodCall.Object);
            var exps = resolver.ExpStores!;
            var joinExp = exps["Join"]!;
            resolver.Sql.Append("GROUP_CONCAT( ");
            if (exps.ContainsKey("Distinct"))
            {
                resolver.Sql.Append("DISTINCT ");
            }
            resolver.Visit(joinExp);
            if (exps.TryGetValue("OrderBy", out var exp))
            {
                resolver.Sql.Append(" ORDER BY ");
                resolver.Visit(exp);
                resolver.Sql.Append(" ASC ");
            }
            else if (exps.TryGetValue("OrderByDesc", out exp))
            {
                resolver.Sql.Append(" ORDER BY ");
                resolver.Visit(exp);
                resolver.Sql.Append(" DESC ");
            }
            if (exps.TryGetValue("Separator", out exp))
            {
                resolver.Sql.Append(" SEPARATOR ");
                resolver.Options.Parameterized = false;
                resolver.Visit(exp);
                resolver.Options.Parameterized = true;
            }
            resolver.Sql.Append(')');
        }
    }
}
