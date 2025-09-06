using LightORM.Providers;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using LightORM.DbStruct;

namespace LightORM.ExpressionSql;

internal abstract class ExpressionCoreSqlBase
{
    public abstract ISqlExecutor Ado { get; }
    public abstract ExpressionSqlOptions Options { get; }
    public IExpSelect<T> Select<T>() => new SelectProvider1<T>(Ado);

    #region insert

    public IExpInsert<T> Insert<T>() => CreateInsertProvider<T>();

    public IExpInsert<T> Insert<T>(params T[] entities)
    {
        if (entities.Length == 1)
        {
            return CreateInsertProvider<T>(entities[0]);
        }
        else
        {
            return CreateInsertProvider<T>(entities);
        }
    }

    InsertProvider<T> CreateInsertProvider<T>(T? entity = default) => new(Ado, entity);
    InsertProvider<T> CreateInsertProvider<T>(IEnumerable<T> entities) => new(Ado, entities);

    #endregion

    #region update

    public IExpUpdate<T> Update<T>() => CreateUpdateProvider<T>();

    public IExpUpdate<T> Update<T>(params T[] entities)
    {
        if (entities.Length == 1)
        {
            return CreateUpdateProvider<T>(entities[0]);
        }
        else
        {
            return CreateUpdateProvider<T>(entities);
        }
    }

    UpdateProvider<T> CreateUpdateProvider<T>(T? entity = default) => new(Ado, entity);
    UpdateProvider<T> CreateUpdateProvider<T>(IEnumerable<T> entities) => new(Ado, entities);

    #endregion

    #region delete

    public IExpDelete<T> Delete<T>() => CreateDeleteProvider<T>();

    public IExpDelete<T> Delete<T>(params T[] entities)
    {
        if (entities.Length == 1)
        {
            return CreateDeleteProvider<T>(entities[0]);
        }
        else
        {
            return CreateDeleteProvider<T>(entities);
        }
    }

    DeleteProvider<T> CreateDeleteProvider<T>(T? entity = default) => new(Ado, entity);
    DeleteProvider<T> CreateDeleteProvider<T>(IEnumerable<T> entities) => new(Ado, entities);

    #endregion

    #region 数据库表操作

    public string? CreateTableSql<T>(Action<TableGenerateOption>? action = null)
    {
        var ado = Ado;
        return InternalCreateTableSql<T>(ado, Options, action);
    }

    public async Task<bool> CreateTableAsync<T>(Action<TableGenerateOption>? action = null, CancellationToken cancellationToken = default)
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

    protected static string InternalCreateTableSql<T>(ISqlExecutor ado, ExpressionSqlOptions option, Action<TableGenerateOption>? action = null)
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
    
    protected static async Task<bool> InternalCreateTableAsync<T>(ISqlExecutor ado, ExpressionSqlOptions options, Action<TableGenerateOption>? action, CancellationToken cancellationToken)
    {
        try
        {
            var sqls = GenerateDbTable<T>(ado, options, action).ToArray();
            if (sqls.Length == 0)
            {
                return false;
            }

            ado.BeginTransaction();
            foreach (var s in sqls)
            {
                await ado.ExecuteNonQueryAsync(s, cancellationToken: cancellationToken);
            }

            await ado.CommitTransactionAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            await ado.RollbackTransactionAsync(cancellationToken);
            return false;
        }
    }
    
    protected static async Task<IList<DbStruct.ReadedTable>> InternalGetTablesAsync(ISqlExecutor ado, ExpressionSqlOptions _)
    {
        if (ado.Database.DbHandler is null)
            return [];
        var sql = ado.Database.DbHandler.GetTablesSql();
        return await ado.QueryListAsync<DbStruct.ReadedTable>(sql);
    }

    protected static async Task<ReadedTable> InternalTableStructAsync(ReadedTable table, ISqlExecutor ado, ExpressionSqlOptions _)
    {
        if (ado.Database.DbHandler is null)
            throw new NotSupportedException();
        var sql = ado.Database.DbHandler.GetTableStructSql(table.TableName);
        var columns = await ado.QueryListAsync<DbStruct.ReadedTableColumn>(sql);
        return table with { Columns = columns };
    }

    private static IEnumerable<string> GenerateDbTable<T>(ISqlExecutor ado, ExpressionSqlOptions option, Action<TableGenerateOption>? action = null)
    {
        if (ado.Database.DbHandler is null)
            return [];
        var o = option.TableGenOption;
        if (action != null)
        {
            o = (TableGenerateOption)o.Clone();
            action(o);
        }

        var tableSql = ado.Database.DbHandler.GenerateDbTable<T>(o);
        return tableSql;
    }

    #endregion
}