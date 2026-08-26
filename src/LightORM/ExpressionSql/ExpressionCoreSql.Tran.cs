using System.Diagnostics;

namespace LightORM.ExpressionSql
{
    partial class ExpressionCoreSql : IExpressionContext
    {
        public ISingleScopedExpressionContext Use(IDatabaseProvider db)
        {

            var connection = connectionFactory.GetDatabaseConnection(db);
            return new SingleScopedExpressionCoreSql(connection, Options);
        }

        public ISingleScopedExpressionContext CreateScoped(string key)
        {
            Debug.WriteLine("CreateScoped");
            var connection = connectionFactory.GetDatabaseConnection(key);
            return new SingleScopedExpressionCoreSql(connection, Options);
        }

        public IScopedExpressionContext CreateScoped()
        {
            return new ScopedExpressionCoreSql(Options);
        }
    }
}