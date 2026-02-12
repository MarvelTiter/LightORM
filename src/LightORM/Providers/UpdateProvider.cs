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

        public UpdateProvider(ISqlExecutor executor, T[] entities)
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

        public IExpUpdate<T> SetNull<TNull>(Expression<Func<T, TNull>> exp)
        {
            sqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                Expression = exp,
                ResolveOptions = SqlResolveOptions.Update,
                AdditionalParameter = new UpdateValue()
            });
            return this;
        }

        public IExpUpdate<T> SetNullIf<TNull>(bool condition, Expression<Func<T, TNull>> exp)
        {
            if (condition)
            {
                SetNull(exp);
            }
            return this;
        }

        public IExpUpdate<T> Set<TField>(Expression<Func<T, TField>> exp, TField value)
        {
            if (exp.Body.NodeType == ExpressionType.New || exp.Body.NodeType == ExpressionType.MemberInit)
            {
                throw new LightOrmException("不支持多字段设置");
            }
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

        public IExpUpdate<T> UpdateByName(string propertyName, object? value = null)
        {
            if ((sqlBuilder.TargetObject is null && sqlBuilder.TargetObjects.Length == 0) && value is null)
            {
                throw new LightOrmException("未设置实体值，并且value是null");
            }
            sqlBuilder.AddMember(propertyName, value);
            return this;
        }

        public IExpUpdate<T> UpdateByNames(string[] propertyNames, object[]? values = null)
        {
            if ((sqlBuilder.TargetObject is null && sqlBuilder.TargetObjects.Length == 0) && values is null)
            {
                throw new LightOrmException("未设置实体值，并且values是null");
            }
            if (values is not null && propertyNames.Length != values.Length)
            {
                throw new LightOrmException("参数数量和列数量不匹配");
            }
            for (int i = 0; i < propertyNames.Length; i++)
            {
                sqlBuilder.AddMember(propertyNames[i], values?[i]);
            }
            return this;
        }

        public string ToSql()
        {
            var sql = sqlBuilder.ToSqlString(Database);
            if (sqlBuilder.IsBatchUpdate)
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
            if (sqlBuilder.IsBatchUpdate)
            {
                foreach (var batch in sqlBuilder.BatchInfos ?? [])
                {
                    sb.AppendLine($"批量更新，批次：{batch.Index}");
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

    }
}
