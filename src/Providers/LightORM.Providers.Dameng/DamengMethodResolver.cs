using LightORM.Extension;
using LightORM.Implements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.Dameng;

public sealed class DamengMethodResolver : BaseSqlMethodResolver
{
    public override void ToString(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        var type = methodCall.Object?.Type;
        if (type != null)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
        }
        var isDatetime = type == typeof(DateTime);
        if (isDatetime)
        {
            resolver.Sql.Append("TO_CHAR(");
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(',');
            if (methodCall.Arguments.Count > 0
                && methodCall.Arguments[0] is ConstantExpression ce
                && ce.Value is string format)
            {
                // yyyy-MM-dd HH:mm:ss
                // YYYY-MM-DD HH24:MI:SS
                resolver.Sql.Append($"'{ConvertFormatString(format)}'");
            }
            else
            {
                resolver.Sql.Append("'YYYY-MM-DD HH24:MI:SS'");
            }
            resolver.Sql.Append(')');
        }
        else
        {
            resolver.Sql.Append("TO_CHAR(");
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(')');
        }
        static string ConvertFormatString(string format)
        {
            return format
                    .Replace("yyyy", "YYYY")
                    .Replace("MM", "MM")
                    .Replace("dd", "DD")
                    .Replace("HH", "HH24")
                    .Replace("mm", "MI")
                    .Replace("ss", "SS");
        }
    }

    public override void StartsWith(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        resolver.Visit(methodCall.Object);
        resolver.Sql.Append(resolver.IsNot ? " NOT LIKE " : " LIKE ");
        resolver.Visit(methodCall.Arguments[0]);
        resolver.Sql.Append(" || '%')");
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
            resolver.Sql.Append("'%' || ");
            resolver.Visit(methodCall.Arguments[0]);
            resolver.Sql.Append(" || '%')");
        }
    }

    public override void EndsWith(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        resolver.Visit(methodCall.Object);
        resolver.Sql.Append(resolver.IsNot ? " NOT LIKE " : " LIKE ");
        resolver.Sql.Append("'%' || ");
        resolver.Visit(methodCall.Arguments[0]);
    }

    public override void Substring(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        // SUBSTRING(columnName, startIndex, endIndex?);
        resolver.Sql.Append("SUBSTR");
        resolver.Sql.Append('(');
        resolver.Visit(methodCall.Object);
        resolver.Sql.Append(',');
        resolver.Sql.Append('(');
        resolver.Visit(methodCall.Arguments[0]);
        // (start + 1)
        resolver.Sql.Append(" + 1");
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
            resolver.Sql.Append("WM_CONCAT( ");
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
            //if (exps.TryGetValue("Separator", out exp))
            //{
            //    resolver.Sql.Append(" SEPARATOR ");
            //    resolver.Options.Parameterized = false;
            //    resolver.Visit(exp);
            //    resolver.Options.Parameterized = true;
            //}
            resolver.Sql.Append(')');
            resolver.ExpStores?.Clear();
        }
    }

    public override void NullThen(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        resolver.Sql.Append("NVL(");
        resolver.Visit(methodCall.Arguments[0]);
        resolver.Sql.Append(',');
        resolver.Visit(methodCall.Arguments[1]);
        resolver.Sql.Append(')');
    }
}
