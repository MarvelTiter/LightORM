using LightORM.Extension;
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
            resolver.Sql.Append("STRFTIME(");
            if (methodCall.Arguments.Count > 0
                && methodCall.Arguments[0] is ConstantExpression ce
                && ce.Value is string format)
            {

                // yyyy-MM-dd HH:mm:ss
                // %Y-%m-%d %H:%M:%S
                //resolver.Sql.Replace("yyyy", "%Y");
                //resolver.Sql.Replace("MM", "%m");
                //resolver.Sql.Replace("dd", "%d");
                //resolver.Sql.Replace("HH", "%H");
                //resolver.Sql.Replace("mm", "%M");
                //resolver.Sql.Replace("ss", "%S");
                resolver.Sql.Append($"'{ConvertFormatString(format)}'");
            }
            else
            {
                resolver.Sql.Append("'%Y-%m-%d %H:%M:%S'");
            }
            resolver.Sql.Append(',');
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(" )");
        }
        else
        {
            resolver.Sql.Append("CAST(");
            resolver.Visit(methodCall.Object);
            if (methodCall.Arguments.Count > 0)
            {
                resolver.Visit(methodCall.Arguments[0]);
            }
            resolver.Sql.Append(" AS TEXT)");
        }
        static string ConvertFormatString(string format)
        {
            return format
                    .Replace("yyyy", "%Y")
                    .Replace("MM", "%m")
                    .Replace("dd", "%d")
                    .Replace("HH", "%H")
                    .Replace("mm", "%I")
                    .Replace("ss", "%S");
        }
    }
    public override void StartsWith(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        resolver.Visit(methodCall.Object);
        resolver.Sql.Append(resolver.IsNot ? " NOT LIKE " : " LIKE ");
        resolver.Visit(methodCall.Arguments[0]);
        resolver.Sql.Append("||'%'");
    }

    public override void Contains(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        if (methodCall.Method.DeclaringType == typeof(string))
        {
            // 字符串
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(resolver.IsNot ? " NOT LIKE " : " LIKE ");
            resolver.Sql.Append("'%'||");
            resolver.Visit(methodCall.Arguments[0]);
            resolver.Sql.Append("||'%'");
        }
        else
        {
            if (methodCall.Method.IsStatic)
            {
                resolver.Visit(methodCall.Arguments[1]);
                resolver.Sql.Append(resolver.IsNot ? " NOT IN " : " IN ");
                resolver.Sql.Append('(');
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(')');
            }
            else 
            {
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(resolver.IsNot ? " NOT IN " : " IN ");
                resolver.Sql.Append('(');
                resolver.Visit(methodCall.Object);
                resolver.Sql.Append(')');
            }
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
            resolver.Visit(joinExp);
            if (exps.TryGetValue("Separator", out var exp))
            {
                resolver.Sql.Append(',');
                resolver.Options.Parameterized = false;
                resolver.Visit(exp);
                resolver.Options.Parameterized = true;
            }
            resolver.Sql.Append(')');
            resolver.ExpStores?.Clear();
        }
    }

    public override void NullThen(IExpressionResolver resolver, MethodCallExpression methodCall)
    {
        resolver.Sql.Append("IFNULL(");
        resolver.Visit(methodCall.Arguments[0]);
        resolver.Sql.Append(',');
        resolver.Visit(methodCall.Arguments[1]);
        resolver.Sql.Append(')');
    }
}
