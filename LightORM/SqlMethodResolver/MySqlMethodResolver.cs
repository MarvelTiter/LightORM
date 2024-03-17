using LightORM.Abstracts;
using System.Linq;

namespace LightORM.SqlMethodResolver
{
    internal class MySqlMethodResolver : SqlMethod
    {
        public MySqlMethodResolver()
        {
            methods.Add(nameof(string.StartsWith), (resolver, methodCall) =>
            {
                resolver.Visit(methodCall.Object);
                resolver.Sql.Append(resolver.IsNot ? " NOT LIKE " : " LIKE ");
                resolver.Sql.Append("CONCAT(");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(", '%')");
            });

            methods.Add(nameof(string.EndsWith), (resolver, methodCall) =>
            {
                resolver.Visit(methodCall.Object);
                resolver.Sql.Append(resolver.IsNot ? " NOT LIKE " : " LIKE ");
                resolver.Sql.Append("CONCAT('%', ");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(")");
            });

            methods.Add("Contains", (resolver, methodCall) =>
            {
                if (methodCall.Object != null && (methodCall.Object.Type.FullName?.StartsWith("System.Collections.Generic") ?? false))
                {
                    resolver.Visit(methodCall.Arguments[0]);
                    resolver.Sql.Append(resolver.IsNot ? " NOT IN " : " IN ");
                    resolver.Sql.Append("(");
                    resolver.Visit(methodCall.Object);
                    resolver.Sql.Append(")");
                }
                else if (methodCall.Method.DeclaringType == typeof(Enumerable))
                {
                    resolver.Visit(methodCall.Arguments[1]);
                    resolver.Sql.Append(resolver.IsNot ? " NOT IN " : " IN ");
                    resolver.Sql.Append("(");
                    resolver.Visit(methodCall.Arguments[0]);
                    resolver.Sql.Append(")");
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
            });

            methods.Add(nameof(string.Substring), (resolver, methodCall) =>
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
            });

            methods.Add(nameof(string.Trim), (resolver, methodCall) =>
            {
                // TRIM(columnName);
                resolver.Sql.Append("TRIM");
                resolver.Sql.Append("( ");
                resolver.Visit(methodCall.Object);
                resolver.Sql.Append(" )");
            });

            methods.Add(nameof(string.TrimStart), (resolver, methodCall) =>
            {
                // LTRIM(columnName);
                resolver.Sql.Append("LTRIM");
                resolver.Sql.Append("( ");
                resolver.Visit(methodCall.Object);
                resolver.Sql.Append(" )");
            });

            methods.Add(nameof(string.TrimEnd), (resolver, methodCall) =>
            {
                // RTRIM(columnName);
                resolver.Sql.Append("RTRIM");
                resolver.Sql.Append("( ");
                resolver.Visit(methodCall.Object);
                resolver.Sql.Append(" )");
            });
        }
    }
}
