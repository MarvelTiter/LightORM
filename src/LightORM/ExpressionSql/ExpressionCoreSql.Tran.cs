using System.Diagnostics;

namespace LightORM.ExpressionSql
{
    partial class ExpressionCoreSql : IExpressionContext
    {
        public ISingleScopedExpressionContext Use(IDatabaseProvider db)
        {

            var connection = connectionFactory.GetDatabaseConnection(db);
            connection.KeepAlive = true;
            return new SingleScopedExpressionCoreSql(connection, Options);
        }

        public ISingleScopedExpressionContext CreateScoped(string key)
        {
            Debug.WriteLine("CreateScoped");
            var connection = connectionFactory.GetDatabaseConnection(key);
            connection.KeepAlive = true;
            return new SingleScopedExpressionCoreSql(connection, Options);
        }

        public IScopedExpressionContext CreateScoped()
        {
            return new ScopedExpressionCoreSql(Options);
        }
    }
}