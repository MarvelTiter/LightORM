using System.Text;
using System.Threading;

namespace LightORM.Providers
{
    internal sealed class DeleteProvider<T> : IExpDelete<T>
    {
        private readonly ISqlExecutor executor;
        private readonly DeleteBuilder<T> sqlBuilder;
        private ICustomDatabase Database => executor.Database.CustomDatabase;
        //public bool ForceDelete { get => sqlBuilder.ForceDelete; set => sqlBuilder.ForceDelete = value; }
        //public bool Truncate { get => sqlBuilder.Truncate; set => sqlBuilder.Truncate = value; }
        public DeleteProvider(ISqlExecutor executor, T? entity)
        {
            this.executor = executor;
            sqlBuilder = new DeleteBuilder<T>();
            sqlBuilder.SelectedTables.Add(TableInfo.Create<T>());
            sqlBuilder.TargetObject = entity;
        }

        public DeleteProvider(ISqlExecutor executor, IEnumerable<T> entities)
        {
            this.executor = executor;
            sqlBuilder = new DeleteBuilder<T>();
            sqlBuilder.SelectedTables.Add(TableInfo.Create<T>());
            sqlBuilder.TargetObjects = entities;
            sqlBuilder.IsBatchDelete = true;
        }


        public int Execute()
        {
            var sql = sqlBuilder.ToSqlString(Database);
            var dbParameters = sqlBuilder.DbParameters;
            return executor.ExecuteNonQuery(sql, dbParameters);
        }

        public Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var sql = sqlBuilder.ToSqlString(Database);
            var dbParameters = sqlBuilder.DbParameters;
            return executor.ExecuteNonQueryAsync(sql, dbParameters, cancellationToken: cancellationToken);
        }

        public string ToSql() => sqlBuilder.ToSqlString(Database);
        public string ToSqlWithParameters()
        {
            var sql = sqlBuilder.ToSqlString(Database);
            StringBuilder sb = new(sql);
            sb.AppendLine();
            sb.AppendLine("参数列表: ");
            foreach (var item in sqlBuilder.DbParameters)
            {
                sb.AppendLine($"{item.Key} - {item.Value}");
            }
            return sb.ToString();
        }

        public IExpDelete<T> Where(Expression<Func<T, bool>> exp)
        {
            sqlBuilder.Expressions.Add(new ExpressionInfo
            {
                ResolveOptions = SqlResolveOptions.DeleteWhere,
                Expression = exp,
            });
            return this;
        }

        public IExpDelete<T> WhereIf(bool condition, Expression<Func<T, bool>> exp)
        {
            if (condition)
            {
                return Where(exp);
            }
            return this;
        }

        public IExpDelete<T> FullDelete(bool truncate = false)
        {
            sqlBuilder.ForceDelete = true;
            sqlBuilder.Truncate = truncate;
            return this;
        }
    }
}
