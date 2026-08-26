using LightORM.DbStruct;
using LightORM.Providers;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace LightORM.ExpressionSql;

internal static class ExpressionCoreSqlContextMethodImpl
{
    public static MultipleResult QueryMultiple(SqlAdo ado, params IExpSelect[] selects)
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
                var originSql = select.SqlBuilder.ToSqlString(ado.Provider.DatabaseAdapter);

                if (select.SqlBuilder.DbParameters.Count > 0)
                {
                    sqls[i] = ado.Provider.DatabaseAdapter.RewriteParameterReferences(originSql, $"q{i}");

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
            var sql = ado.Provider.DatabaseAdapter.HandleMultipleQuerySql(sqls, parameters);
            var reader = ado.ExecuteReader(sql, parameters);
            return new MultipleResult(reader);
        }
        finally
        {
#if NET8_0_OR_GREATER
            System.Buffers.ArrayPool<string>.Shared.Return(sqls);
#endif
        }
    }

    public static async Task<MultipleResult> QueryMultipleAsync(SqlAdo ado, IExpSelect[] selects, CancellationToken cancellationToken = default)
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
                var originSql = select.SqlBuilder.ToSqlString(ado.Provider.DatabaseAdapter);

                if (select.SqlBuilder.DbParameters.Count > 0)
                {
                    sqls[i] = ado.Provider.DatabaseAdapter.RewriteParameterReferences(originSql, $"q{i}");

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
            var sql = ado.Provider.DatabaseAdapter.HandleMultipleQuerySql(sqls, parameters);
            var reader = await ado.ExecuteReaderAsync(sql, parameters, cancellationToken: cancellationToken);
            return new MultipleResult(reader);
        }
        finally
        {
#if NET8_0_OR_GREATER
            System.Buffers.ArrayPool<string>.Shared.Return(sqls);
#endif
        }
    }

    public static IExpSelect<T> Select<
#if NET8_0_OR_GREATER
       [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    T>(IContext context) => new SelectProvider1<T>(context);

    #region insert

    public static IExpInsert<T> Insert<T>(SqlAdo ado) => CreateInsertProvider<T>(ado);

    public static IExpInsert<T> Insert<T>(SqlAdo ado, params T[] entities)
    {
        if (entities.Length == 0)
        {
            return CreateInsertProvider(ado, default(T));
        }
        if (entities.Length == 1)
        {
            return CreateInsertProvider(ado, entities[0]);
        }
        else
        {
            return CreateInsertProvider(ado, entities);
        }
    }

    static InsertProvider<T> CreateInsertProvider<T>(SqlAdo ado, T? entity = default) => new(ado, entity);
    static InsertProvider<T> CreateInsertProvider<T>(SqlAdo ado, T[] entities) => new(ado, entities);

    #endregion

    #region update

    public static IExpUpdate<T> Update<T>(SqlAdo ado) => CreateUpdateProvider<T>(ado);

    public static IExpUpdate<T> Update<T>(SqlAdo ado, params T[] entities)
    {
        if (entities.Length == 0)
        {
            return CreateUpdateProvider(ado, default(T));
        }
        if (entities.Length == 1)
        {
            return CreateUpdateProvider(ado, entities[0]);
        }
        else
        {
            return CreateUpdateProvider(ado, entities);
        }
    }

    static UpdateProvider<T> CreateUpdateProvider<T>(SqlAdo ado, T? entity = default) => new(ado, entity);
    static UpdateProvider<T> CreateUpdateProvider<T>(SqlAdo ado, T[] entities) => new(ado, entities);

    #endregion

    #region delete

    public static IExpDelete<T> Delete<T>(SqlAdo ado) => CreateDeleteProvider<T>(ado);

    public static IExpDelete<T> Delete<T>(SqlAdo ado, params T[] entities)
    {
        if (entities.Length == 1)
        {
            return CreateDeleteProvider(ado, entities[0]);
        }
        else
        {
            return CreateDeleteProvider(ado, entities);
        }
    }

    static DeleteProvider<T> CreateDeleteProvider<T>(SqlAdo ado, T? entity = default) => new(ado, entity);
    static DeleteProvider<T> CreateDeleteProvider<T>(SqlAdo ado, T[] entities) => new(ado, entities);

    #endregion

    #region 数据库表操作

    public static string InternalCreateTableSql<T>(IDatabaseProvider provider, Action<TableOptions>? action = null)
    {
        try
        {
            return string.Join(Environment.NewLine, GenerateDbTable<T>(provider, action));
        }
        catch (Exception)
        {
            throw;
        }
    }

    public static async Task<bool> InternalCreateTableAsync<T>(SqlAdo ado, ExpressionSqlOptions options, Action<TableOptions>? action, CancellationToken cancellationToken)
    {
        try
        {
            var sqls = GenerateDbTable<T>(ado.Provider, action).ToArray();
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

    public static async Task<IList<ReadedTable>> InternalGetTablesAsync(SqlAdo ado, ExpressionSqlOptions _)
    {
        if (ado.Provider.DbHandler is null)
            return [];
        var sql = ado.Provider.DbHandler.GetTablesSql();
        return await ado.Execute(sql).ToListAsync<ReadedTable>();
    }

    public static async Task<ReadedTable> InternalTableStructAsync(ReadedTable table, SqlAdo ado, ExpressionSqlOptions _)
    {
        if (ado.Provider.DbHandler is null)
            throw new NotSupportedException();
        var sql = ado.Provider.DbHandler.GetTableStructSql(table.TableName!);
        var columns = await ado.Execute(sql).ToListAsync<ReadedTableColumn>();
        return table with { Columns = columns };
    }

    public static async Task<bool> InternalDropTableAsync(SqlAdo ado, string tableName, CancellationToken cancellationToken)
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

    private static IEnumerable<string> GenerateDbTable<T>(IDatabaseProvider provider, Action<TableOptions>? action = null)
    {
        if (provider.DbHandler is null)
            return [];
        var tableSql = provider.DbHandler.GenerateDbTable<T>();
        return tableSql;
    }

    #endregion
}
