using System.Text;

namespace LightORM.Providers.Select
{
    internal class SelectInsertProvider<T> : ISelectInsert<T>
    {
        private readonly ISqlExecutor executor;
        private readonly SelectBuilder builder;
        private ICustomDatabase Database => executor.Database.CustomDatabase;
        public SelectInsertProvider(ISqlExecutor executor, SelectBuilder builder)
        {
            this.executor = executor;
            this.builder = builder;
        }
        public int Execute()
        {
            var sql = builder.ToSqlString(Database);
            return executor.ExecuteNonQuery(sql, builder.DbParameters);
        }

        public Task<int> ExecuteAsync()
        {
            var sql = builder.ToSqlString(Database);
            return executor.ExecuteNonQueryAsync(sql, builder.DbParameters);
        }

        public string ToSql() => builder.ToSqlString(Database);

        public string ToSqlWithParameters()
        {
            var sql = builder.ToSqlString(Database);
            StringBuilder sb = new(sql);
            sb.AppendLine();
            sb.AppendLine("参数列表: ");
            foreach (var item in builder.DbParameters)
            {
                sb.AppendLine($"{item.Key} - {item.Value}");
            }
            return sb.ToString();
        }
    }
}
