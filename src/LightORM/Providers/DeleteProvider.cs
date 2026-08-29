using LightORM.Extension;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace LightORM.Providers
{
    internal sealed class DeleteProvider<T> : IExpDelete<T>
    {
        private readonly SqlAdo ado;
        private readonly DeleteBuilder<T> sqlBuilder;
        private IDatabaseAdapter Database => ado.Provider.DatabaseAdapter;
        //public bool ForceDelete { get => sqlBuilder.ForceDelete; set => sqlBuilder.ForceDelete = value; }
        //public bool Truncate { get => sqlBuilder.Truncate; set => sqlBuilder.Truncate = value; }
        public DeleteProvider(SqlAdo executor, T? entity)
        {
            this.ado = executor;
            sqlBuilder = new();
            sqlBuilder.AddTableInfo(TableInfo.Create<T>());
            sqlBuilder.TargetObject = entity;
        }

        public DeleteProvider(SqlAdo executor, T[] entities)
        {
            this.ado = executor;
            sqlBuilder = new();
            sqlBuilder.AddTableInfo(TableInfo.Create<T>());
            sqlBuilder.TargetObjects = entities;
            sqlBuilder.IsBatchDelete = true;
        }

        public void UpdateTableName(string tableName) => sqlBuilder.MainTable.OverriddenTableName = tableName;

        #region 自定义控制

        public IExpDelete<T> NoQuoteIdentifiers()
        {
            sqlBuilder.QuoteIdentifiers = false;
            return this;
        }

        public IExpDelete<T> QuoteIdentifiers()
        {
            sqlBuilder.QuoteIdentifiers = true;
            return this;
        }

        #endregion

        #region 日志输出辅助

        public IExpDelete<T> TagWith(string tag)
        {
            sqlBuilder.AddTag(new(tag, null, null, null, false));
            return this;
        }
        public IExpDelete<T> TagWithCallSite(string tag, [CallerFilePath] string? filePath = null, [CallerMemberName] string? callMember = null, [CallerLineNumber] int? lineNum = null)
        {
            sqlBuilder.AddTag(new(tag, filePath, callMember, lineNum, true));
            return this;
        }

        #endregion

        public int Execute()
        {
            var sql = sqlBuilder.ToSqlString(Database);
            if (sqlBuilder.IsBatchDelete)
            {
                try
                {
                    var effectRows = 0;
                    if (ado.Connection.UnderTransaction)
                        ado.Connection.BeginTransaction();
                    foreach (var item in sqlBuilder.BatchInfos!)
                    {
                        effectRows += ado.ExecuteNonQuery(item.Sql!, item.ToDictionaryParameters());
                    }
                    if (ado.Connection.UnderTransaction)
                        ado.Connection.CommitTransaction();
                    return effectRows;
                }
                catch
                {
                    if (ado.Connection.UnderTransaction)
                        ado.Connection.RollbackTransaction();
                    throw;
                }

            }
            else
            {
                var dbParameters = sqlBuilder.DbParameters;
                return ado.ExecuteNonQuery(sql, dbParameters);
            }
        }

        public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            //var sql = sqlBuilder.ToSqlString(Database);
            //var dbParameters = sqlBuilder.DbParameters;
            //return executor.ExecuteNonQueryAsync(sql, dbParameters, cancellationToken: cancellationToken);
            var sql = sqlBuilder.ToSqlString(Database);
            if (sqlBuilder.IsBatchDelete)
            {
                try
                {
                    var effectRows = 0;
                    if (ado.Connection.UnderTransaction)
                    {
#if NET8_0_OR_GREATER
                        await ado.Connection.BeginTransactionAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
#else
                        ado.Connection.BeginTransaction();
#endif
                    }
                    foreach (var item in sqlBuilder.BatchInfos!)
                    {
                        effectRows += await ado.ExecuteNonQueryAsync(item.Sql!, item.ToDictionaryParameters(), cancellationToken: cancellationToken).ConfigureAwait(false);
                    }
                    if (ado.Connection.UnderTransaction)
                    {
#if NET8_0_OR_GREATER
                        await ado.Connection.CommitTransactionAsync(cancellationToken).ConfigureAwait(false);
#else
                        ado.Connection.CommitTransaction();
#endif
                    }
                    return effectRows;
                }
                catch
                {
                    if (ado.Connection.UnderTransaction)
                    {
#if NET8_0_OR_GREATER
                        await ado.Connection.RollbackTransactionAsync(cancellationToken).ConfigureAwait(false);
#else
                        ado.Connection.RollbackTransaction();
#endif
                    }
                    throw;
                }

            }
            else
            {
                var dbParameters = sqlBuilder.DbParameters;
                return await ado.ExecuteNonQueryAsync(sql, dbParameters, cancellationToken: cancellationToken);
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
                    foreach (var item in batch.RowParameters)
                    {
                        sb.AppendLine("----行数据");
                        item.ForEach(row =>
                        {
                            if (row.IsStaticValue) return;
                            sb.AppendLine($"--------{row.ValueName} - {row.Value}");
                        });
                    }
                }
            }
            return sb.ToString();
        }

        public IExpDelete<T> Where(Expression<Func<T, bool>> exp)
        {
            sqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.DeleteWhere, exp));
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
