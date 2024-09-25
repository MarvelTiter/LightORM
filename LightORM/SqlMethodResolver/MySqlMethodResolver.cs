using System.Linq;

namespace LightORM.SqlMethodResolver
{
    internal class MySqlMethodResolver : SqlMethod
    {
        public MySqlMethodResolver()
        {
            #region Count, Sum
            methods.Add(nameof(SqlFn.Count), (resolver, methodCall) =>
            {
                if (methodCall.Arguments.Count > 0)
                {
                    var useCaseWhen = methodCall.Method.GetParameters()[0].ParameterType == typeof(bool)
                        && methodCall.Arguments[0] is BinaryExpression;
                    if (useCaseWhen)
                    {
                        resolver.Sql.Append("COUNT( CASE WHEN ");
                        resolver.Visit(methodCall.Arguments[0]);
                        resolver.Sql.Append(" THEN 1 ElSE null END )");
                    }
                    else
                    {
                        resolver.Sql.Append("COUNT( ");
                        resolver.Visit(methodCall.Arguments[0]);
                        resolver.Sql.Append(" )");
                    }
                }
                else
                {
                    resolver.Sql.Append("COUNT(*)");
                }
            });
            methods.Add(nameof(SqlFn.Sum), (resolver, methodCall) =>
            {
                if (methodCall.Arguments.Count > 1)
                {
                    resolver.Sql.Append("SUM( CASE WHEN ");
                    resolver.Visit(methodCall.Arguments[0]);
                    resolver.Sql.Append(" THEN ");
                    resolver.Visit(methodCall.Arguments[1]);
                    resolver.Sql.Append(" ElSE 0 END )");
                }
                else
                {
                    resolver.Sql.Append("SUM(");
                    resolver.Visit(methodCall.Arguments[0]);
                    resolver.Sql.Append(')');
                }
            });
            methods.Add(nameof(SqlFn.Avg), (resolver, methodCall) =>
            {
                if (methodCall.Arguments.Count > 1)
                {
                    resolver.Sql.Append("AVG( CASE WHEN ");
                    resolver.Visit(methodCall.Arguments[0]);
                    resolver.Sql.Append(" THEN ");
                    resolver.Visit(methodCall.Arguments[1]);
                    resolver.Sql.Append(" ElSE 0 END )");
                }
                else
                {
                    resolver.Sql.Append("AVG(");
                    resolver.Visit(methodCall.Arguments[0]);
                    resolver.Sql.Append(')');
                }
            });
            #endregion

            #region Like, Trim
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
            #endregion
        }
    }
}
