using System.Threading;
using LightORM.Providers;
namespace LightORM.ExpressionSql;

internal sealed partial class ExpressionCoreSql : ExpressionCoreSqlBase, IExpressionContext, IDisposable
{
    private readonly ExpressionSqlOptions option;
    internal SqlExecutorProvider executorProvider;
    public string Id { get; } = $"{Guid.NewGuid():N}";
    private readonly SemaphoreSlim switchSign = new(1, 1);
    public override ISqlExecutor Ado => executorProvider.GetSqlExecutor(CurrentKey, UseTrans);

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

    //public IExpSelect<T> Select<T>() => new SelectProvider1<T>(Ado);

    //public IExpInsert<T> Insert<T>() => CreateInsertProvider<T>();
    //public IExpInsert<T> Insert<T>(T entity) => CreateInsertProvider<T>(entity);
    //public IExpInsert<T> Insert<T>(IEnumerable<T> entities) => CreateInsertProvider<T>(entities);

    //InsertProvider<T> CreateInsertProvider<T>(T? entity = default)
    //{
    //    return new(Ado, entity);
    //}
    //InsertProvider<T> CreateInsertProvider<T>(IEnumerable<T> entities)
    //{
    //    return new(Ado, entities);
    //}

    //public IExpUpdate<T> Update<T>() => CreateUpdateProvider<T>();
    //public IExpUpdate<T> Update<T>(T entity) => CreateUpdateProvider<T>(entity);
    //public IExpUpdate<T> Update<T>(IEnumerable<T> entities) => CreateUpdateProvider<T>(entities);

    //UpdateProvider<T> CreateUpdateProvider<T>(T? entity = default)
    //{
    //    return new(Ado, entity);
    //}
    //UpdateProvider<T> CreateUpdateProvider<T>(IEnumerable<T> entities)
    //{
    //    return new(Ado, entities);
    //}

    //public IExpDelete<T> Delete<T>() => CreateDeleteProvider<T>();
    //public IExpDelete<T> Delete<T>(T entity) => CreateDeleteProvider<T>(entity);
    //public IExpDelete<T> Delete<T>(IEnumerable<T> entities) => CreateDeleteProvider<T>(entities);

    //DeleteProvider<T> CreateDeleteProvider<T>(T? entity = default)
    //{
    //    return new(Ado, entity);
    //}
    //DeleteProvider<T> CreateDeleteProvider<T>(IEnumerable<T> entities)
    //{
    //    return new(Ado, entities);
    //}

    private bool disposedValue;

    private void Dispose(bool disposing)
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

internal abstract class ExpressionCoreSqlBase
{
    public abstract ISqlExecutor Ado { get; }
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
}

internal sealed class ScopedExpressionCoreSql : ExpressionCoreSqlBase, IScopedExpressionContext
{
    private readonly SqlExecutorProvider executorProvider;
    public string Id { get; } = $"{Guid.NewGuid():N}";
    private string? dbKey;
    //class FinallyAction : IDisposable
    //{
    //    private readonly Action action;

    //    public FinallyAction(Action action)
    //    {
    //        this.action = action;
    //    }
    //    public void Dispose()
    //    {
    //        action.Invoke();
    //    }
    //}
    public override ISqlExecutor Ado
    {
        get
        {
            //using var _ = new FinallyAction(() => dbKey = null);
            if (dbKey is null)
            {
                return executorProvider.GetSqlExecutor(ConstString.Main, true);
            }
            var ado = executorProvider.GetSqlExecutor(dbKey, true);
            dbKey = null;
            return ado;
        }
    }

    public ScopedExpressionCoreSql(ExpressionSqlOptions options)
    {
        this.executorProvider = new SqlExecutorProvider(options);
    }
    IScopedExpressionContext IScopedExpressionContext.SwitchDatabase(string key)
    {
        dbKey = key;
        return this;
    }

    public void CommitTran()
    {
        executorProvider.Executors.ForEach(e => e.CommitTran());
    }

    public async Task CommitTranAsync()
    {
        await executorProvider.Executors.ForEachAsync(e => e.CommitTranAsync());
    }

    public void RollbackTran()
    {
        executorProvider.Executors.ForEach(e => e.RollbackTran());
    }

    public async Task RollbackTranAsync()
    {
        await executorProvider.Executors.ForEachAsync(e => e.RollbackTranAsync());
    }

    public void Dispose()
    {
        RollbackTran();
        executorProvider.Dispose();
    }
}

internal sealed class SingleScopedExpressionCoreSql : ExpressionCoreSqlBase, ISingleScopedExpressionContext
{
    public string Id { get; } = $"{Guid.NewGuid():N}";
    public override ISqlExecutor Ado { get; }
    public SingleScopedExpressionCoreSql(ISqlExecutor sqlExecutor)
    {
        Ado = sqlExecutor;
    }
    public void CommitTran() => Ado.CommitTran();

    public Task CommitTranAsync() => Ado.CommitTranAsync();

    public void RollbackTran() => Ado.RollbackTran();

    public Task RollbackTranAsync() => Ado.RollbackTranAsync();

    public void Dispose()
    {
        Ado.RollbackTran();
        Ado.Dispose();
    }
}