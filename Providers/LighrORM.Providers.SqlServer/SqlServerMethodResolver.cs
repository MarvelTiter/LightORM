using LightORM;
using LightORM.Implements;
using System.Linq.Expressions;

namespace LighrORM.Providers.SqlServer;

public sealed class SqlServerMethodResolver : BaseSqlMethodResolver
{
    public SqlServerVersion Version { get; }

    public SqlServerMethodResolver(SqlServerVersion version)
    {
        Version = version;
    }
    public override void StartsWith(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        resolver.Visit(methodCall.Object);
        resolver.Sql.Append(resolver.IsNot ? " NOT LIKE " : " LIKE ");
        resolver.Visit(methodCall.Arguments[0]);
        resolver.Sql.Append("+'%'");
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
            resolver.Sql.Append("'%'+");
            resolver.Visit(methodCall.Arguments[0]);
            resolver.Sql.Append("+'%'");
        }
    }

    public override void EndsWith(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        resolver.Visit(methodCall.Object);
        resolver.Sql.Append(resolver.IsNot ? " NOT LIKE " : " LIKE ");
        resolver.Sql.Append("'%'+");
        resolver.Visit(methodCall.Arguments[0]);
    }

    public override void Substring(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        // SUBSTRING(columnName, startIndex, endIndex?);
        resolver.Sql.Append("SUBSTRING");
        resolver.Sql.Append("( ");
        resolver.Visit(methodCall.Object);
        resolver.Sql.Append(',');
        resolver.Visit(methodCall.Arguments[0]);
        if (methodCall.Arguments.Count > 1)
        {
            resolver.Sql.Append(',');
            resolver.Visit(methodCall.Arguments[1]);
        }
        resolver.Sql.Append(" )");
    }

    public override void Trim(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        if (Version == SqlServerVersion.Over2017)
        {
            // TRIM(columnName);
            resolver.Sql.Append("TRIM");
            resolver.Sql.Append('(');
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(')');
        }
        else
        {
            base.Trim(resolver, methodCall);
        }
    }

    public override void TrimStart(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        // LTRIM(columnName);
        resolver.Sql.Append("LTRIM");
        resolver.Sql.Append('(');
        resolver.Visit(methodCall.Object);
        resolver.Sql.Append(')');
    }

    public override void TrimEnd(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        // RTRIM(columnName);
        resolver.Sql.Append("RTRIM");
        resolver.Sql.Append('(');
        resolver.Visit(methodCall.Object);
        resolver.Sql.Append(')');
    }
}
