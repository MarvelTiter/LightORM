using System.Threading;
using LightORM.Providers;
namespace LightORM.ExpressionSql;

public partial class ExpressionCoreSql : IExpressionContext, IDisposable
{
    private readonly ExpressionSqlOptions option;
    internal SqlExecutorProvider executorProvider;
    public string Id { get; } = $"{Guid.NewGuid():N}";
    private readonly SemaphoreSlim switchSign = new(1, 1);
    public ISqlExecutor Ado
    {
        get
        {
            var e = executorProvider.GetSqlExecutor(CurrentKey, false);
            return e;
        }
    }

    public ExpressionCoreSql(ExpressionSqlOptions option)
    {
#if DEBUG
        Console.WriteLine($"创建ExpressionCoreSql：{DateTime.Now}");
#endif
        executorProvider = new SqlExecutorProvider(option);
        this.option = option;
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

    //public IExpSelect<T> Select<T>() => Select<T>(t => t!);

    public IExpSelect Select(string tableName) => throw new NotImplementedException();//new SelectProvider0(tableName, Ado);

    public IExpSelect<T> Select<T>() => CreateSelectProvider<T>();

    SelectProvider1<T> CreateSelectProvider<T>()
    {
        var table = TableContext.GetTableInfo<T>();
        if (table.TargetDatabase != null)
        {
            _dbKey = table.TargetDatabase;
        }
        return new(Ado);
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

internal class ScopedExpressionCoreSql(ISqlExecutor ado) : IScopedExpressionContext
{
    public string Id { get; } = $"{Guid.NewGuid():N}";

    public ISqlExecutor Ado { get; } = (ISqlExecutor)ado.Clone();

    public IExpSelect<T> Select<T>() => new SelectProvider1<T>(Ado);

    public IExpInsert<T> Insert<T>() => CreateInsertProvider<T>();
    public IExpInsert<T> Insert<T>(T entity) => CreateInsertProvider<T>(entity);
    public IExpInsert<T> Insert<T>(IEnumerable<T> entities) => CreateInsertProvider<T>(entities);
    InsertProvider<T> CreateInsertProvider<T>(T? entity = default) => new(Ado, entity);
    InsertProvider<T> CreateInsertProvider<T>(IEnumerable<T> entities) => new(Ado, entities);

    public IExpUpdate<T> Update<T>() => CreateUpdateProvider<T>();
    public IExpUpdate<T> Update<T>(T entity) => CreateUpdateProvider<T>(entity);
    public IExpUpdate<T> Update<T>(IEnumerable<T> entities) => CreateUpdateProvider<T>(entities);
    UpdateProvider<T> CreateUpdateProvider<T>(T? entity = default) => new(Ado, entity);
    UpdateProvider<T> CreateUpdateProvider<T>(IEnumerable<T> entities) => new(Ado, entities);

    public IExpDelete<T> Delete<T>() => CreateDeleteProvider<T>();
    public IExpDelete<T> Delete<T>(T entity) => CreateDeleteProvider<T>(entity);
    public IExpDelete<T> Delete<T>(IEnumerable<T> entities) => CreateDeleteProvider<T>(entities);
    DeleteProvider<T> CreateDeleteProvider<T>(T? entity = default) => new(Ado, entity);
    DeleteProvider<T> CreateDeleteProvider<T>(IEnumerable<T> entities) => new(Ado, entities);

    public void CommitTran()
    {
        Ado.CommitTran();
    }

    public async Task CommitTranAsync()
    {
        await Ado.CommitTranAsync();
    }

    public void RollbackTran()
    {
        Ado.RollbackTran();
    }

    public async Task RollbackTranAsync()
    {
        await Ado.RollbackTranAsync();
    }

    ~ScopedExpressionCoreSql()
    {
        Ado.Dispose();
    }
}