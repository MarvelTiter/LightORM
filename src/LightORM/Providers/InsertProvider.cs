using LightORM.Extension;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace LightORM.Providers;

internal sealed class InsertProvider<T> : IExpInsert<T>
{
    private readonly SqlAdo ado;
    private readonly InsertBuilder<T> sqlBuilder;
    private IDatabaseAdapter Database => ado.Provider.DatabaseAdapter;
    public InsertProvider(SqlAdo executor, T? entity)
    {
        this.ado = executor;
        sqlBuilder = new();
        sqlBuilder.SelectedTables.Add(TableInfo.Create<T>());
        sqlBuilder.TargetObject = entity;
    }

    public InsertProvider(SqlAdo executor, T[] entities)
    {
        this.ado = executor;
        sqlBuilder = new();
        sqlBuilder.SelectedTables.Add(TableInfo.Create<T>());
        sqlBuilder.TargetObjects = entities;
        sqlBuilder.IsBatchInsert = true;
    }

    public void UpdateTableName(string tableName) => sqlBuilder.MainTable.OverriddenTableName = tableName;

    #region 自定义控制

    public IExpInsert<T> NoQuoteIdentifiers()
    {
        sqlBuilder.QuoteIdentifiers = false;
        return this;
    }

    public IExpInsert<T> QuoteIdentifiers()
    {
        sqlBuilder.QuoteIdentifiers = true;
        return this;
    }

    #endregion

    #region 日志输出辅助

    public IExpInsert<T> TagWith(string tag)
    {
        sqlBuilder.AddTag(new(tag, null, null, null, false));
        return this;
    }
    public IExpInsert<T> TagWithCallSite(string tag, [CallerFilePath] string? filePath = null, [CallerMemberName] string? callMember = null, [CallerLineNumber] int? lineNum = null)
    {
        sqlBuilder.AddTag(new(tag, filePath, callMember, lineNum, true));
        return this;
    }

    #endregion

    public void SetTargetObject(T? entity)
    {
        sqlBuilder.TargetObject = entity;
        sqlBuilder.DbParameters.Clear();
        sqlBuilder.IsBatchInsert = false;
    }

    public IExpInsert<T> IgnoreColumns<TIgnore>(Expression<Func<T, TIgnore>> columns)
    {
        sqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.InsertIgnore, columns));
        return this;
    }

    public IExpInsert<T> NoParameter()
    {
        return this;
    }

    public IExpInsert<T> ReturnIdentity()
    {
        sqlBuilder.IsReturnIdentity = true;
        return this;
    }

    public IExpInsert<T> InsertColumns<TColumns>(Expression<Func<T, TColumns>> columns)
    {
        sqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.Insert, columns));
        return this;
    }

    public IExpInsert<T> Set<TField>(Expression<Func<T, TField>> field, TField value)
    {
        if (field.Body.NodeType == ExpressionType.New || field.Body.NodeType == ExpressionType.MemberInit)
        {
            throw new LightOrmException("不支持多字段设置");
        }
        sqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.Insert, field, additionalParameter: new SpecificValue() { Value = value }));

        return this;
    }

    public IExpInsert<T> SetIf<TField>(bool condition, Expression<Func<T, TField>> field, TField value)
    {
        if (condition)
        {
            return Set(field, value);
        }
        return this;
    }

    public IExpInsert<T> InsertByName(string propertyName, object? value = null)
    {
        if ((sqlBuilder.TargetObject is null && sqlBuilder.TargetObjects.Length == 0) && value is null)
        {
            throw new LightOrmException("未设置实体值，并且value是null");
        }
        sqlBuilder.AddMember(propertyName, value);
        return this;
    }

    public IExpInsert<T> InsertByNames(string[] propertyNames, object[]? values = null)
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

    public IExpInsert<T> OrUpdate<Columns>(Expression<Func<T, bool>>? where, Expression<Func<T, Columns>>? columns)
    {
        if (columns is not null)
        {
            sqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.Insert, columns));
        }
        if (where is not null)
        {
            sqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.UpdateWhere, where));
        }
        sqlBuilder.UpdateOnConflict = true;
        return this;
    }

    public IExpInsert<T> IgnoreIfExits()
    {
        sqlBuilder.IgnoreOnConflict = true;
        return this;
    }

    public int Execute()
    {
        var sql = sqlBuilder.ToSqlString(Database);
        if (sqlBuilder.IsBatchInsert)
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
        var sql = sqlBuilder.ToSqlString(Database);
        if (sqlBuilder.IsBatchInsert)
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
            var parameters = sqlBuilder.DbParameters;
            return await ado.ExecuteNonQueryAsync(sql, parameters, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    public string ToSql()
    {
        var sql = sqlBuilder.ToSqlString(Database);
        if (sqlBuilder.IsBatchInsert)
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
        if (sqlBuilder.IsBatchInsert)
        {
            foreach (var batch in sqlBuilder.BatchInfos ?? [])
            {
                sb.AppendLine($"批量插入，批次：{batch.Index}");
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
