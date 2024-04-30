using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LightORM.Cache;
using LightORM.Providers;
using LightORM.Providers.Select;
using LightORM.Utils;
namespace LightORM.ExpressionSql;

public partial class ExpressionCoreSql : IExpressionContext, IDisposable
{
    internal SqlExecutorProvider executorProvider;
    SemaphoreSlim switchSign = new SemaphoreSlim(1, 1);
    bool useCustom;
    public ISqlExecutor Ado
    {
        get
        {
            var e = executorProvider.GetSelectExecutor(_dbKey ?? MainDb, useCustom);
            if (switchSign.CurrentCount == 0) switchSign.Release();
            useCustom = false;
            return e;
        }
    }

    public ExpressionCoreSql(ExpressionSqlOptions option)
    {
#if DEBUG
        Console.WriteLine($"创建ExpressionCoreSql：{DateTime.Now}");
#endif
        executorProvider = new SqlExecutorProvider(option.Aop);
    }

    private readonly string MainDb = ConstString.Main;
    private string? _dbKey = null;

    internal string CurrentKey
    {
        get
        {
            var k = _dbKey ?? MainDb;
            _dbKey = null;
            if (switchSign.CurrentCount == 0) switchSign.Release();
            return k;
        }
    }


    public IExpressionContext SwitchDatabase(string key)
    {
        // 确保切换key之后，Provider拿到的ISqlExecutor是对应的
        switchSign.Wait();
        _dbKey = key;
        return this;
    }

    public IExpSelect<T> Select<T>() => Select<T>(t => t!);

    public IExpSelect Select(string tableName) => new SelectProvider0(tableName, Ado);

    public IExpSelect<T> Select<T>(Expression<Func<T, object>> exp) => CreateSelectProvider<T>(exp.Body);

    Providers.Select.SelectProvider1<T> CreateSelectProvider<T>(Expression body)
    {
        var table = TableContext.GetTableInfo<T>();
        if (table.TargetDatabase != null)
        {
            _dbKey = table.TargetDatabase;
        }
        return new(body, Ado);
    }

    public IExpInsert<T> Insert<T>() => CreateInsertProvider<T>();
    public IExpInsert<T> Insert<T>(T entity) => CreateInsertProvider<T>(entity);
    public IExpInsert<T> Insert<T>(IEnumerable<T> entities) => CreateInsertProvider<T>(entities);

    InsertProvider<T> CreateInsertProvider<T>(T? entity = default)
    {
        var table = TableContext.GetTableInfo<T>();
        if (table.TargetDatabase != null)
        {
            _dbKey = table.TargetDatabase;
        }
        return new(executorProvider.GetSqlExecutor(CurrentKey, UseTrans), entity);
    }
    InsertProvider<T> CreateInsertProvider<T>(IEnumerable<T> entities)
    {
        var table = TableContext.GetTableInfo<T>();
        if (table.TargetDatabase != null)
        {
            _dbKey = table.TargetDatabase;
        }
        return new(executorProvider.GetSqlExecutor(CurrentKey, UseTrans), entities);
    }

    public IExpUpdate<T> Update<T>() => CreateUpdateProvider<T>();
    public IExpUpdate<T> Update<T>(T entity) => CreateUpdateProvider<T>(entity);
    public IExpUpdate<T> Update<T>(IEnumerable<T> entities) => CreateUpdateProvider<T>(entities);

    UpdateProvider<T> CreateUpdateProvider<T>(T? entity = default)
    {
        var table = TableContext.GetTableInfo<T>();
        if (table.TargetDatabase != null)
        {
            _dbKey = table.TargetDatabase;
        }
        return new(executorProvider.GetSqlExecutor(CurrentKey, UseTrans), entity);
    }
    UpdateProvider<T> CreateUpdateProvider<T>(IEnumerable<T> entities)
    {
        var table = TableContext.GetTableInfo<T>();
        if (table.TargetDatabase != null)
        {
            _dbKey = table.TargetDatabase;
        }
        return new(executorProvider.GetSqlExecutor(CurrentKey, UseTrans), entities);
    }

    public IExpDelete<T> Delete<T>() => CreateDeleteProvider<T>();
    public IExpDelete<T> Delete<T>(T entity) => CreateDeleteProvider<T>(entity);
    public IExpDelete<T> Delete<T>(IEnumerable<T> entities) => CreateDeleteProvider<T>(entities);

    DeleteProvider<T> CreateDeleteProvider<T>(T? entity = default)
    {
        var table = TableContext.GetTableInfo<T>();
        if (table.TargetDatabase != null)
        {
            _dbKey = table.TargetDatabase;
        }
        return new(executorProvider.GetSqlExecutor(CurrentKey, UseTrans), entity);
    }
    DeleteProvider<T> CreateDeleteProvider<T>(IEnumerable<T> entities)
    {
        var table = TableContext.GetTableInfo<T>();
        if (table.TargetDatabase != null)
        {
            _dbKey = table.TargetDatabase;
        }
        return new(executorProvider.GetSqlExecutor(CurrentKey, UseTrans), entities);
    }



    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
#if DEBUG
                Console.WriteLine($"释放ExpressionCoreSql：{DateTime.Now}");
#endif
                executorProvider.Dispose();
            }

            disposedValue = true;
        }
    }
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }


}