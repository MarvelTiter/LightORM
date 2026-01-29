using LightORM.Extension;
using System.Text;
using System.Threading;

namespace LightORM.Providers
{
    internal class UpdateProvider<T> : IExpUpdate<T>
    {
        private readonly ISqlExecutor executor;
        private readonly UpdateBuilder<T> sqlBuilder;
        private ICustomDatabase Database => executor.Database.CustomDatabase;
        public UpdateProvider(ISqlExecutor executor, T? entity)
        {
            this.executor = executor;
            sqlBuilder = new UpdateBuilder<T>();
            sqlBuilder.SelectedTables.Add(TableInfo.Create<T>());
            sqlBuilder.TargetObject = entity;
        }

        public UpdateProvider(ISqlExecutor executor, IEnumerable<T> entities)
        {
            this.executor = executor;
            sqlBuilder = new UpdateBuilder<T>();
            sqlBuilder.SelectedTables.Add(TableInfo.Create<T>());
            sqlBuilder.IsBatchUpdate = true;
            sqlBuilder.TargetObjects = entities;
        }

        public int Execute()
        {
            var sql = sqlBuilder.ToSqlString(Database);
            if (sqlBuilder.IsBatchUpdate)
            {
                var usingTransaction = executor.DbTransaction != null;
                try
                {
                    var effectRows = 0;
                    if (usingTransaction)
                    {
                        executor.BeginTransaction();
                    }
                    foreach (var item in sqlBuilder.BatchInfos!)
                    {
                        effectRows += executor.ExecuteNonQuery(item.Sql!, item.ToDictionaryParameters());
                    }
                    if (usingTransaction)
                    {
                        executor.CommitTransaction();
                    }
                    return effectRows;
                }
                catch
                {
                    if (usingTransaction)
                    {
                        executor.RollbackTransaction();
                    }
                    throw;
                }

            }
            else
            {
                var dbParameters = sqlBuilder.DbParameters;
                return executor.ExecuteNonQuery(sql, dbParameters);
            }
        }

        public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var sql = sqlBuilder.ToSqlString(Database);
            if (sqlBuilder.IsBatchUpdate)
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

        public IExpUpdate<T> SetNull<TField>(Expression<Func<T, TField>> exp)
        {
            //var result = exp.Resolve(SqlResolveOptions.Update, SqlBuilder.MainTable);
            //var member = result.Members!.First();
            //SqlBuilder.SetNullMembers.Add(member);
            sqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                Expression = exp,
                ResolveOptions = SqlResolveOptions.Update,
                AdditionalParameter = new UpdateValue()
            });
            return this;
        }

        public IExpUpdate<T> SetNullIf<TField>(bool condition, Expression<Func<T, TField>> exp)
        {
            if (condition)
            {
                SetNull(exp);
            }
            return this;
        }

        public IExpUpdate<T> Set<TField>(Expression<Func<T, TField>> exp, TField value)
        {

            //var result = exp.Resolve(SqlResolveOptions.Update, SqlBuilder.MainTable);
            //var member = result.Members!.First();
            //if (value is null)
            //{
            //    SqlBuilder.SetNullMembers.Add(member);
            //}
            //else
            //{
            //    SqlBuilder.Members.Add(member);
            //    SqlBuilder.DbParameters.Add(member, value!);
            //}

            sqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                Expression = exp,
                ResolveOptions = SqlResolveOptions.Update,
                AdditionalParameter = new UpdateValue() { Value = value }
            });

            return this;
        }

        public IExpUpdate<T> SetIf<TField>(bool condition, Expression<Func<T, TField>> exp, TField value)
        {
            if (condition)
            {
                return Set(exp, value);
            }
            return this;
        }

        public IExpUpdate<T> UpdateColumns(Expression<Func<object>> columns)
        {
            sqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                Expression = columns,
                ResolveOptions = SqlResolveOptions.Update,
            });
            return this;
        }
        public IExpUpdate<T> UpdateColumns<TUpdate>(Expression<Func<T, TUpdate>> columns)
        {
            sqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                Expression = columns,
                ResolveOptions = SqlResolveOptions.Update,
            });
            return this;
        }

        public IExpUpdate<T> IgnoreColumns<TIgnore>(Expression<Func<T, TIgnore>> columns)
        {
            sqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                Expression = columns,
                ResolveOptions = SqlResolveOptions.UpdateIgnore
            });
            return this;
        }

        public IExpUpdate<T> Where(Expression<Func<T, bool>> exp)
        {
            sqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                Expression = exp,
                ResolveOptions = SqlResolveOptions.UpdateWhere
            });
            return this;
        }

        public IExpUpdate<T> WhereIf(bool condition, Expression<Func<T, bool>> exp)
        {
            if (condition)
            {
                return Where(exp);
            }
            return this;
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

    }
}
