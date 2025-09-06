using LightORM.Extension;
using LightORM.Implements;
using System.Linq.Expressions;

namespace LightORM.Providers.SqlServer;

public sealed class SqlServerMethodResolver(SqlServerVersion version) : BaseSqlMethodResolver
{
    public SqlServerVersion Version { get; } = version;

    public override void ToString(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        var type = methodCall.Object?.Type;
        if (type != null)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
        }
        var isDatetime = type == typeof(DateTime);
        resolver.Sql.Append("CONVERT(VARCHAR(MAX),");
        resolver.Visit(methodCall.Object);
        if (isDatetime)
        {
            resolver.Sql.Append(',');
            int styleCode = 120; // 默认使用ODBC规范
            if (methodCall.Arguments.Count > 0
                && methodCall.Arguments[0] is ConstantExpression { Value: string format })
            {
                // 映射常见格式字符串到SQL Server样式代码
                styleCode = format switch
                {
                    // 日期时间格式
                    "yyyy-MM-dd HH:mm:ss" => 120,  // ODBC规范
                    "yyyy-MM-ddTHH:mm:ss" => 126,   // ISO8601
                    "MM/dd/yyyy HH:mm:ss" => 22,    // U.S.格式
                    // 纯日期格式
                    "yyyy-MM-dd" => 23,
                    "MM/dd/yyyy" => 101,            // U.S.
                    "dd/MM/yyyy" => 103,            // 英国/法国
                    "yyyyMMdd" => 112,              // ISO
                    "dd MMM yyyy" => 106,           // 欧洲默认 - 26 Sep 2023
                    // 纯时间格式
                    "HH:mm:ss" => 114,
                    "hh:mm:ss tt" => 0,             // 默认时间格式
                    // 其他常见格式
                    "MMMM dd, yyyy" => 107,         // 月份全名 - September 26, 2023
                    _ => 120                        // 默认使用ODBC格式
                };
            }
            resolver.Sql.Append(styleCode);
        }
        else if (methodCall.Arguments.Count > 0)
        {
            resolver.Visit(methodCall.Arguments[0]);
        }
        resolver.Sql.Append(')');
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

    //public override void Join(IExpressionResolver resolver, MethodCallExpression methodCall)
    //{
    //    if (Version == SqlServerVersion.Over2017)
    //    {
    //        resolver.Sql.Append("STRING_AGG(");
    //        resolver.Visit(methodCall.Arguments[0]);
    //        if (methodCall.Arguments.Count > 1)
    //        {
    //            resolver.Sql.Append(", ");
    //            resolver.Options.Parameterized = false;
    //            resolver.Visit(methodCall.Arguments[1]);
    //            resolver.Options.Parameterized = true;
    //        }
    //        resolver.Sql.Append(')');
    //    }
    //    base.Join(resolver, methodCall);
    //}

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
            if (Version < SqlServerVersion.Over2017)
            {
                throw new NotSupportedException("不支持STRING_AGG函数");
            }
            resolver.Visit(methodCall.Object);
            var exps = resolver.ExpStores!;
            var joinExp = exps["Join"]!;
            resolver.Sql.Append("STRING_AGG( ");
            resolver.Visit(joinExp);
            if (exps.TryGetValue("Separator", out var exp))
            {
                resolver.Sql.Append(", ");
                resolver.Options.Parameterized = false;
                resolver.Visit(exp);
                resolver.Options.Parameterized = true;
            }
            else
            {
                resolver.Sql.Append(", ','");
            }
            resolver.Sql.Append(')');

            if (exps.TryGetValue("OrderBy", out exp))
            {
                resolver.Sql.Append(" WITHIN GROUP (ORDER BY ");
                resolver.Visit(exp);
                resolver.Sql.Append(" ASC)");
            }
            else if (exps.TryGetValue("OrderByDesc", out exp))
            {
                resolver.Sql.Append(" WITHIN GROUP (ORDER BY ");
                resolver.Visit(exp);
                resolver.Sql.Append(" DESC)");
            }
            resolver.ExpStores?.Clear();
        }
    }

    public override void NullThen(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        resolver.Sql.Append("ISNULL(");
        resolver.Visit(methodCall.Arguments[0]);
        resolver.Sql.Append(',');
        resolver.Visit(methodCall.Arguments[1]);
        resolver.Sql.Append(')');
    }
}
