using LightORM.DbStruct;
using LightORM.Providers;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace LightORM.ExpressionSql;

internal abstract class ExpressionCoreSqlBase(ExpressionSqlOptions options) : IContext
{
    public abstract SqlAdo Ado { get; }
    public ExpressionSqlOptions Options { get; } = options;

    public MultipleResult QueryMultiple(params IExpSelect[] selects)
    {
        if (selects.Length == 0)
        {
            throw new LightOrmException("selects 数量为0");
        }
#if NET8_0_OR_GREATER
        string[] sqls = System.Buffers.ArrayPool<string>.Shared.Rent(selects.Length);
#else
        string[] sqls = new string[selects.Length];
#endif
        Dictionary<string, object> parameters = [];
        try
        {
            for (var i = 0; i < selects.Length; i++)
            {
                var select = selects[i];
                var originSql = select.SqlBuilder.ToSqlString(Ado.Provider.DatabaseAdapter);

                if (select.SqlBuilder.DbParameters.Count > 0)
                {
                    sqls[i] = Ado.Provider.DatabaseAdapter.RewriteParameterReferences(originSql, $"q{i}");

                    foreach (var item in select.SqlBuilder.DbParameters)
                    {
                        parameters[$"q{i}_{item.Key}"] = item.Value;
                    }
                }
                else
                {
                    sqls[i] = originSql;
                }
            }
            var sql = Ado.Provider.DatabaseAdapter.HandleMultipleQuerySql(sqls, parameters);
            var reader = Ado.ExecuteReader(sql, parameters);
            return new MultipleResult(reader);
        }
        finally
        {
#if NET8_0_OR_GREATER
            System.Buffers.ArrayPool<string>.Shared.Return(sqls);
#endif
        }
    }

    public async Task<MultipleResult> QueryMultipleAsync(IExpSelect[] selects, CancellationToken cancellationToken = default)
    {
        if (selects.Length == 0)
        {
            throw new LightOrmException("selects 数量为0");
        }
#if NET8_0_OR_GREATER
        string[] sqls = System.Buffers.ArrayPool<string>.Shared.Rent(selects.Length);
#else
        string[] sqls = new string[selects.Length];
#endif
        Dictionary<string, object> parameters = [];
        try
        {
            for (var i = 0; i < selects.Length; i++)
            {
                var select = selects[i];
                var originSql = select.SqlBuilder.ToSqlString(Ado.Provider.DatabaseAdapter);

                if (select.SqlBuilder.DbParameters.Count > 0)
                {
                    sqls[i] = Ado.Provider.DatabaseAdapter.RewriteParameterReferences(originSql, $"q{i}");

                    foreach (var item in select.SqlBuilder.DbParameters)
                    {
                        parameters[$"q{i}_{item.Key}"] = item.Value;
                    }
                }
                else
                {
                    sqls[i] = originSql;
                }
            }
            var sql = Ado.Provider.DatabaseAdapter.HandleMultipleQuerySql(sqls, parameters);
            var reader = await Ado.ExecuteReaderAsync(sql, parameters, cancellationToken: cancellationToken);
            return new MultipleResult(reader);
        }
        finally
        {
#if NET8_0_OR_GREATER
            System.Buffers.ArrayPool<string>.Shared.Return(sqls);
#endif
        }
    }

    public IExpSelect<T> Select<
#if NET8_0_OR_GREATER
       [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    T>() => new SelectProvider1<T>(this);

    #region insert

    public IExpInsert<T> Insert<T>() => CreateInsertProvider<T>();

    public IExpInsert<T> Insert<T>(params T[] entities)
    {
        if (entities.Length == 0)
        {
            return CreateInsertProvider(default(T));
        }
        if (entities.Length == 1)
        {
            return CreateInsertProvider(entities[0]);
        }
        else
        {
            return CreateInsertProvider(entities);
        }
    }

    InsertProvider<T> CreateInsertProvider<T>(T? entity = default) => new(Ado, entity);
    InsertProvider<T> CreateInsertProvider<T>(T[] entities) => new(Ado, entities);

    #endregion

    #region update

    public IExpUpdate<T> Update<T>() => CreateUpdateProvider<T>();

    public IExpUpdate<T> Update<T>(params T[] entities)
    {
        if (entities.Length == 0)
        {
            return CreateUpdateProvider(default(T));
        }
        if (entities.Length == 1)
        {
            return CreateUpdateProvider(entities[0]);
        }
        else
        {
            return CreateUpdateProvider(entities);
        }
    }

    UpdateProvider<T> CreateUpdateProvider<T>(T? entity = default) => new(Ado, entity);
    UpdateProvider<T> CreateUpdateProvider<T>(T[] entities) => new(Ado, entities);

    #endregion

    #region delete

    public IExpDelete<T> Delete<T>() => CreateDeleteProvider<T>();

    public IExpDelete<T> Delete<T>(params T[] entities)
    {
        if (entities.Length == 1)
        {
            return CreateDeleteProvider(entities[0]);
        }
        else
        {
            return CreateDeleteProvider(entities);
        }
    }

    DeleteProvider<T> CreateDeleteProvider<T>(T? entity = default) => new(Ado, entity);
    DeleteProvider<T> CreateDeleteProvider<T>(T[] entities) => new(Ado, entities);

    #endregion

    #region 数据库表操作

    public string? CreateTableSql<T>(Action<TableOptions>? action = null)
    {
        var ado = Ado;
        return InternalCreateTableSql<T>(ado, Options, action);
    }

    public async Task<bool> CreateTableAsync<T>(Action<TableOptions>? action = null, CancellationToken cancellationToken = default)
    {
        var ado = Ado;
        return await InternalCreateTableAsync<T>(ado, Options, action, cancellationToken);
    }

    public async Task<IList<DbStruct.ReadedTable>> GetTablesAsync()
    {
        var ado = Ado;
        return await InternalGetTablesAsync(ado, Options);
    }

    public async Task<DbStruct.ReadedTable> GetTableStructAsync(DbStruct.ReadedTable table)
    {
        var ado = Ado;
        return await InternalTableStructAsync(table, ado, Options);
    }

    public async Task<bool> DropTableAsync<T>(CancellationToken cancellationToken = default)
    {
        var ado = Ado;
        var t = TableContext.GetTableInfo<T>();
        return await InternalDropTableAsync(ado, t.TableName, cancellationToken);
    }

    protected static string InternalCreateTableSql<T>(SqlAdo ado, ExpressionSqlOptions option, Action<TableOptions>? action = null)
    {
        try
        {
            return string.Join(Environment.NewLine, GenerateDbTable<T>(ado, option, action));
        }
        catch (Exception)
        {
            throw;
        }
    }

    protected static async Task<bool> InternalCreateTableAsync<T>(SqlAdo ado, ExpressionSqlOptions options, Action<TableOptions>? action, CancellationToken cancellationToken)
    {
        try
        {
            var sqls = GenerateDbTable<T>(ado, options, action).ToArray();
            if (sqls.Length == 0)
            {
                return false;
            }

            //ado.BeginTransaction();
            foreach (var s in sqls)
            {
                await ado.ExecuteNonQueryAsync(s, cancellationToken: cancellationToken);
            }

            //await ado.CommitTransactionAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            //await ado.RollbackTransactionAsync(cancellationToken);
            return false;
        }
    }

    protected static async Task<IList<ReadedTable>> InternalGetTablesAsync(SqlAdo ado, ExpressionSqlOptions _)
    {
        if (ado.Provider.DbHandler is null)
            return [];
        var sql = ado.Provider.DbHandler.GetTablesSql();
        return await ado.Execute(sql).ToListAsync<ReadedTable>();
    }

    protected static async Task<ReadedTable> InternalTableStructAsync(ReadedTable table, SqlAdo ado, ExpressionSqlOptions _)
    {
        if (ado.Provider.DbHandler is null)
            throw new NotSupportedException();
        var sql = ado.Provider.DbHandler.GetTableStructSql(table.TableName!);
        var columns = await ado.Execute(sql).ToListAsync<ReadedTableColumn>();
        return table with { Columns = columns };
    }

    protected static async Task<bool> InternalDropTableAsync(SqlAdo ado, string tableName, CancellationToken cancellationToken)
    {
        try
        {
            if (ado.Provider.DbHandler is null)
                throw new NotSupportedException();
            var sql = ado.Provider.DbHandler.GetDropTableSql(tableName);
            await ado.ExecuteNonQueryAsync(sql, cancellationToken: cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }

    }

    private static IEnumerable<string> GenerateDbTable<T>(SqlAdo ado, ExpressionSqlOptions option, Action<TableOptions>? action = null)
    {
        if (ado.Provider.DbHandler is null)
            return [];
        var tableSql = ado.Provider.DbHandler.GenerateDbTable<T>();
        return tableSql;
    }

    #endregion
}