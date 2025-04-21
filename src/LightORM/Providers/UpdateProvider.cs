using LightORM.Builder;
using LightORM.Extension;
using LightORM.Interfaces.ExpSql;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LightORM.Providers
{
    internal class UpdateProvider<T> : IExpUpdate<T>
    {
        private readonly ISqlExecutor executor;
        private readonly UpdateBuilder<T> SqlBuilder;

        public UpdateProvider(ISqlExecutor executor, T? entity)
        {
            this.executor = executor;
            SqlBuilder = new UpdateBuilder<T>(this.executor.Database.DbBaseType);
            SqlBuilder.SelectedTables.Add(TableInfo.Create<T>());
            SqlBuilder.TargetObject = entity;
        }

        public UpdateProvider(ISqlExecutor executor, IEnumerable<T> entities)
        {
            this.executor = executor;
            SqlBuilder = new UpdateBuilder<T>(this.executor.Database.DbBaseType);
            SqlBuilder.SelectedTables.Add(TableInfo.Create<T>());
            SqlBuilder.IsBatchUpdate = true;
            SqlBuilder.TargetObjects = entities;
        }

        public int Execute()
        {
            var sql = SqlBuilder.ToSqlString();
            if (SqlBuilder.IsBatchUpdate)
            {
                var isTran = executor.DbTransaction == null;
                try
                {
                    var effectRows = 0;
                    if (!isTran)
                    {
                        executor.BeginTran();
                        isTran = true;
                    }
                    foreach (var item in SqlBuilder.BatchInfos!)
                    {
                        effectRows += executor.ExecuteNonQuery(item.Sql!, item.ToDictionaryParameters());
                    }
                    if (isTran)
                    {
                        executor.CommitTran();
                    }
                    return effectRows;
                }
                catch
                {
                    if (isTran)
                    {
                        executor.RollbackTran();
                    }
                    throw;
                }

            }
            else
            {
                var dbParameters = SqlBuilder.DbParameters;
                return executor.ExecuteNonQuery(sql, dbParameters);
            }
        }

        public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var sql = SqlBuilder.ToSqlString();
            if (SqlBuilder.IsBatchUpdate)
            {
                var isTran = executor.DbTransaction == null;
                try
                {
                    var effectRows = 0;
                    if (!isTran)
                    {
                        await executor.BeginTranAsync(cancellationToken);
                        isTran = true;
                    }
                    foreach (var item in SqlBuilder.BatchInfos!)
                    {
                        effectRows += await executor.ExecuteNonQueryAsync(item.Sql!, item.ToDictionaryParameters(), cancellationToken: cancellationToken);
                    }
                    if (isTran)
                    {
                        await executor.CommitTranAsync(cancellationToken);
                    }
                    return effectRows;
                }
                catch
                {
                    if (isTran)
                    {
                        await executor.RollbackTranAsync(cancellationToken);
                    }
                    throw;
                }

            }
            else
            {
                var dbParameters = SqlBuilder.DbParameters;
                return await executor.ExecuteNonQueryAsync(sql, dbParameters, cancellationToken: cancellationToken);
            }
        }

        public IExpUpdate<T> SetNull<TField>(Expression<Func<T, TField>> exp)
        {
            //var result = exp.Resolve(SqlResolveOptions.Update, SqlBuilder.MainTable);
            //var member = result.Members!.First();
            //SqlBuilder.SetNullMembers.Add(member);
            SqlBuilder.Expressions.Add(new ExpressionInfo()
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

            SqlBuilder.Expressions.Add(new ExpressionInfo()
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
            SqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                Expression = columns,
                ResolveOptions = SqlResolveOptions.Update,
            });
            return this;
        }
        public IExpUpdate<T> UpdateColumns(Expression<Func<T, object>> columns)
        {
            SqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                Expression = columns,
                ResolveOptions = SqlResolveOptions.Update,
            });
            return this;
        }

        public IExpUpdate<T> IgnoreColumns(Expression<Func<T, object>> columns)
        {
            SqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                Expression = columns,
                ResolveOptions = SqlResolveOptions.UpdateIgnore
            });
            return this;
        }

        public IExpUpdate<T> Where(Expression<Func<T, bool>> exp)
        {
            SqlBuilder.Expressions.Add(new ExpressionInfo()
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
        public string ToSql() => SqlBuilder.ToSqlString();


    }
}
