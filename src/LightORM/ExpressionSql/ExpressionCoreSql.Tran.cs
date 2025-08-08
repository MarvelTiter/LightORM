using System.Diagnostics;

namespace LightORM.ExpressionSql
{
    partial class ExpressionCoreSql : IExpressionContext
    {

        public ISingleScopedExpressionContext Use(IDatabaseProvider db)
        {
            // 确保Use之后，拿到的ISqlExecutor是对应的
            var ado = new SqlExecutor.SqlExecutor(db, Options.PoolSize, new AdoInterceptor(Options.Interceptors));
            return new SingleScopedExpressionCoreSql(ado);
        }
        
        public ISingleScopedExpressionContext CreateScoped(string key)
        {
            Debug.WriteLine("CreateScoped");
            var ado = (ISqlExecutor)executorProvider.GetSqlExecutor(key).Clone();
            return new SingleScopedExpressionCoreSql(ado);
        }

        public IScopedExpressionContext CreateScoped()
        {
            return new ScopedExpressionCoreSql(Options);
        }

    }
}
