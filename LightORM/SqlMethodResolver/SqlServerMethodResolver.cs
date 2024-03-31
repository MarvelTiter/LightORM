using System.Linq;

namespace LightORM.SqlMethodResolver
{
    internal class SqlServerMethodResolver : SqlMethod
    {
        public SqlServerMethodResolver()
        {
            #region Count, Sum
            methods.Add(nameof(SqlFn.Count), (resolver, methodCall) =>
            {
                resolver.Sql.Append("COUNT( CASE WHEN ");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(" THEN 1 ElSE 0 END )");
            });
            methods.Add(nameof(SqlFn.Sum), (resolver, methodCall) =>
            {
                resolver.Sql.Append("SUM( CASE WHEN ");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(" THEN ");
                resolver.Visit(methodCall.Arguments[1]);
                resolver.Sql.Append(" ElSE 0 END )");
            });
            methods.Add(nameof(SqlFn.Avg), (resolver, methodCall) =>
            {
                resolver.Sql.Append("AVG( CASE WHEN ");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(" THEN ");
                resolver.Visit(methodCall.Arguments[1]);
                resolver.Sql.Append(" ElSE 0 END )");
            });
            #endregion

            #region Like, Trim
            methods.Add(nameof(string.StartsWith), (resolver, methodCall) =>
            {
                resolver.Visit(methodCall.Object);
                resolver.Sql.Append(resolver.IsNot ? " NOT LIKE " : " LIKE ");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append("+'%'");
            });

            methods.Add(nameof(string.EndsWith), (resolver, methodCall) =>
            {
                resolver.Visit(methodCall.Object);
                resolver.Sql.Append(resolver.IsNot ? " NOT LIKE " : " LIKE ");
                resolver.Sql.Append("'%'+");
                resolver.Visit(methodCall.Arguments[0]);
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
                    resolver.Sql.Append("'%'+");
                    resolver.Visit(methodCall.Arguments[0]);
                    resolver.Sql.Append("+'%'");
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
