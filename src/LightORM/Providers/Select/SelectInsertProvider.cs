using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.Select
{
    internal class SelectInsertProvider<T> : ISelectInsert<T>
    {
        private readonly ISqlExecutor executor;
        private readonly SelectBuilder builder;

        public SelectInsertProvider(ISqlExecutor executor, SelectBuilder builder)
        {
            this.executor = executor;
            this.builder = builder;
        }
        public int Execute()
        {
            var sql = builder.ToSqlString();
            return executor.ExecuteNonQuery(sql, builder.DbParameters);
        }

        public Task<int> ExecuteAsync()
        {
            var sql = builder.ToSqlString();
            return executor.ExecuteNonQueryAsync(sql, builder.DbParameters);
        }

        public string ToSql() => builder.ToSqlString();
    }
}
