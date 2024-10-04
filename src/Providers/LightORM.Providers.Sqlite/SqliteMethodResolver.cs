using LightORM.Implements;
using LightORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.Sqlite;

public sealed class SqliteMethodResolver : BaseSqlMethodResolver
{
    public override void StartsWith(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        resolver.Visit(methodCall.Object);
        resolver.Sql.Append(resolver.IsNot ? " NOT LIKE " : " LIKE ");
        resolver.Visit(methodCall.Arguments[0]);
        resolver.Sql.Append("||'%'");
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
            resolver.Sql.Append("'%'||");
            resolver.Visit(methodCall.Arguments[0]);
            resolver.Sql.Append("||'%'");
        }
    }

    public override void EndsWith(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        resolver.Visit(methodCall.Object);
        resolver.Sql.Append(resolver.IsNot ? " NOT LIKE " : " LIKE ");
        resolver.Sql.Append("'%'||");
        resolver.Visit(methodCall.Arguments[0]);
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

    public override void Join(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        resolver.Sql.Append("GROUP_CONCAT( ");
        resolver.Visit(methodCall.Arguments[0]);
        if (methodCall.Arguments.Count > 1)
        {
            resolver.Sql.Append(", ");
            resolver.Options.Parameterized = false;
            resolver.Visit(methodCall.Arguments[1]);
            resolver.Options.Parameterized = true;
        }
    }
}
