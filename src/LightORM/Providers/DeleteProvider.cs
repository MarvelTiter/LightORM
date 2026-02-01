using LightORM.Extension;
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

        public DeleteProvider(ISqlExecutor executor, T[] entities)
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

        public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            //var sql = sqlBuilder.ToSqlString(Database);
            //var dbParameters = sqlBuilder.DbParameters;
            //return executor.ExecuteNonQueryAsync(sql, dbParameters, cancellationToken: cancellationToken);
            var sql = sqlBuilder.ToSqlString(Database);
            if (sqlBuilder.IsBatchDelete)
            {
                var usingTransaction = executor.DbTransaction == null;
                try
                {
                    var effectRows = 0;
                    if (usingTransaction)
                    {
                        executor.BeginTransaction();
                    }
                    foreach (var item in sqlBuilder.BatchInfos!)
                    {
                        effectRows += await executor.ExecuteNonQueryAsync(item.Sql!, item.ToDictionaryParameters(), cancellationToken: cancellationToken).ConfigureAwait(false);
                    }
                    if (usingTransaction)
                    {
                        await executor.CommitTransactionAsync(cancellationToken).ConfigureAwait(false);
                    }
                    return effectRows;
                }
                catch
                {
                    if (usingTransaction)
                    {
                        await executor.RollbackTransactionAsync(cancellationToken).ConfigureAwait(false);
                    }
                    throw;
                }

            }
            else
            {
                var dbParameters = sqlBuilder.DbParameters;
                return await executor.ExecuteNonQueryAsync(sql, dbParameters, cancellationToken: cancellationToken);
            }

        }

        public string ToSql()
        {
            var sql = sqlBuilder.ToSqlString(Database);
            if (sqlBuilder.IsBatchDelete)
            {
                return string.Join($";{Environment.NewLine}", sqlBuilder.BatchInfos?.Select(b => b.Sql) ?? []);
            }
            return sql;
        }
        public string ToSqlWithParameters()
        {
            var sql = ToSql();
            StringBuilder sb = new(sql);
            sb.AppendLine();
            sb.AppendLine("参数列表: ");
            foreach (var item in sqlBuilder.DbParameters)
            {
                sb.AppendLine($"{item.Key} - {item.Value}");
            }
            if (sqlBuilder.IsBatchDelete)
            {
                foreach (var batch in sqlBuilder.BatchInfos ?? [])
                {
                    sb.AppendLine($"批量删除，批次：{batch.Index}");
                    foreach (var item in batch.Parameters)
                    {
                        sb.AppendLine("----行数据");
                        item.ForEach(row =>
                        {
                            if (row.isStaticValue) return;
                            sb.AppendLine($"--------{row.ParameterName} - {row.Value}");
                        });
                    }
                }
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
            sqlBuilder.FullDelete = true;
            sqlBuilder.Truncate = truncate;
            return this;
        }
    }
}
