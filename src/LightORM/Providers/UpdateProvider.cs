using LightORM.Extension;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace LightORM.Providers
{
    internal class UpdateProvider<T> : IExpUpdate<T>
    {
        private readonly SqlAdo ado;
        private readonly UpdateBuilder<T> sqlBuilder;
        private IDatabaseAdapter Database => ado.Provider.DatabaseAdapter;
        public UpdateProvider(SqlAdo executor, T? entity)
        {
            this.ado = executor;
            sqlBuilder = new();
            sqlBuilder.SelectedTables.Add(TableInfo.Create<T>());
            sqlBuilder.TargetObject = entity;
        }

        public UpdateProvider(SqlAdo executor, T[] entities)
        {
            this.ado = executor;
            sqlBuilder = new();
            sqlBuilder.SelectedTables.Add(TableInfo.Create<T>());
            sqlBuilder.IsBatchUpdate = true;
            sqlBuilder.TargetObjects = entities;
        }

        public void UpdateTableName(string tableName) => sqlBuilder.MainTable.OverriddenTableName = tableName;

        #region 自定义控制

        public IExpUpdate<T> NoQuoteIdentifiers()
        {
            sqlBuilder.QuoteIdentifiers = false;
            return this;
        }

        public IExpUpdate<T> QuoteIdentifiers()
        {
            sqlBuilder.QuoteIdentifiers = true;
            return this;
        }

        #endregion

        #region 日志输出辅助

        public IExpUpdate<T> TagWith(string tag)
        {
            sqlBuilder.AddTag(new(tag, null, null, null, false));
            return this;
        }
        public IExpUpdate<T> TagWithCallSite(string tag, [CallerFilePath] string? filePath = null, [CallerMemberName] string? callMember = null, [CallerLineNumber] int? lineNum = null)
        {
            sqlBuilder.AddTag(new(tag, filePath, callMember, lineNum, true));
            return this;
        }

        #endregion

        public int Execute()
        {
            var sql = sqlBuilder.ToSqlString(Database);
            if (sqlBuilder.IsBatchUpdate)
            {
                try
                {
                    var effectRows = 0;
                    if (ado.Connection.UnderTransaction)
                    {
                        ado.Connection.BeginTransaction();
                    }
                    foreach (var item in sqlBuilder.BatchInfos!)
                    {
                        effectRows += ado.ExecuteNonQuery(item.Sql!, item.ToDictionaryParameters());
                    }
                    if (ado.Connection.UnderTransaction)
                    {
                        ado.Connection.CommitTransaction();
                    }
                    return effectRows;
                }
                catch
                {
                    if (ado.Connection.UnderTransaction)
                    {
                        ado.Connection.RollbackTransaction();
                    }
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
            var sql = sqlBuilder.ToSqlString(Database);
            if (sqlBuilder.IsBatchUpdate)
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
        public IExpUpdate<T> SetNullIf<TNull>(bool condition, Expression<Func<T, TNull>> exp)
        {
            if (condition)
            {
                SetNull(exp);
            }
            return this;
        }

        public IExpUpdate<T> SetNull<TNull>(Expression<Func<T, TNull>> exp)
        {
            sqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.Update, exp, additionalParameter: new SpecificValue()));
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

        public IExpUpdate<T> Set<TField>(Expression<Func<T, TField>> exp, TField value)
        {
            if (exp.Body.NodeType == ExpressionType.New || exp.Body.NodeType == ExpressionType.MemberInit)
            {
                throw new LightOrmException("不支持多字段设置");
            }

            sqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.Update, exp, additionalParameter: new SpecificValue() { Value = value }));

            return this;
        }

        public IExpUpdate<T> Set(Expression<Func<T, bool>> exp)
        {
            if (exp.Body is not MethodCallExpression && exp.Body is not BinaryExpression)
            {
                throw new LightOrmException("Set表达式必须是二元表达式，如 p => p.PropertyName == value。或者方法调用");
            }
            if (exp.Body is BinaryExpression binary && binary.NodeType != ExpressionType.Equal)
            {
                throw new InvalidOperationException($"Set表达式只支持相等操作(==)，不支持{binary.NodeType}操作");
            }
            sqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.Update, exp));
            return this;
        }

        public IExpUpdate<T> UpdateColumns(Expression<Func<object>> columns)
        {
            //sqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.Update, columns));
            //return this;
            throw new NotImplementedException();
        }

        public IExpUpdate<T> UpdateColumns<TUpdate>(Expression<Func<T, TUpdate>> columns)
        {
            sqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.Update, columns));
            return this;
        }

        public IExpUpdate<T> IgnoreColumns<TIgnore>(Expression<Func<T, TIgnore>> columns)
        {
            sqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.UpdateIgnore, columns));
            return this;
        }

        public IExpUpdate<T> WithVersion<TVersion>(Expression<Func<T, TVersion>> versionField, TVersion versionValue)
        {
            sqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.UpdateVer, versionField, additionalParameter: new SpecificValue() { Value = versionValue }));
            return this;
        }

        public IExpUpdate<T> WithVersion(object versionValue)
        {
            sqlBuilder.VersionInfo = new() { VersionValue = versionValue };
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

        public IExpUpdate<T> Where(Expression<Func<T, bool>> exp)
        {
            sqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.UpdateWhere, exp));
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

    }
}
