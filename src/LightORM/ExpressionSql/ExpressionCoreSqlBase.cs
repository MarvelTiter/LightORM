using LightORM.Providers;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
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
        try
        {
            return string.Join(Environment.NewLine, GenerateDbTable<T>(ado, Options, action));
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> CreateTableAsync<T>(Action<TableGenerateOption>? action = null, CancellationToken cancellationToken = default)
    {
        var ado = Ado;
        try
        {
            var sqls = GenerateDbTable<T>(ado, Options, action);
            if (!sqls.Any())
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

    public async Task<IList<string>> GetTablesAsync()
    {
        var ado = Ado;
        if (ado.Database.TableHandler is not null)
        {
            var handler = ado.Database.TableHandler.Invoke(Options.TableGenOption);
            var sql = handler.GetTablesSql();
            return await ado.QueryListAsync<string>(sql);
        }
        return [];
    }

    public Task<DbStruct.ReadedTable> GetTableStructAsync(string table)
    {
        throw new NotImplementedException();
    }

    private static IEnumerable<string> GenerateDbTable<T>(ISqlExecutor ado, ExpressionSqlOptions option, Action<TableGenerateOption>? action = null)
    {
        if (ado.Database.TableHandler is not null)
        {
            var o = option.TableGenOption;
            if (action != null)
            {
                o = (TableGenerateOption)o.Clone();
                action(o);
            }
            var handler = ado.Database.TableHandler.Invoke(o);
            var tableSql = handler.GenerateDbTable<T>();
            return tableSql;
        }
        return [];
    }

    #endregion
}

