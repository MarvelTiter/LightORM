using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;
namespace LightORM.SqlExecutor;

internal abstract class ObjectPool<T> : IDisposable
    where T : class, IDisposable
{
    private readonly Func<T> _createFunc;
    private readonly int _maxCapacity;
    private int _numItems;
    private protected readonly ConcurrentQueue<ObjectInfo> _items = new();
    private protected T? _fastItem;
    private bool disposedValue;
    private readonly Timer _healthCheckTimer;
    private protected readonly object _maintenanceLock = new();
    public readonly struct ObjectInfo(T obj)
    {
        public T Object { get; } = obj;
        public DateTimeOffset Created { get; } = DateTimeOffset.Now;
    }
    public ObjectPool(Func<T> func, int maxCapacity)
    {
        _createFunc = func;
        _maxCapacity = maxCapacity;
        _healthCheckTimer = new Timer(HealthCheckCallback, null,
            TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public virtual T Get()
    {
#if NET8_0_OR_GREATER
        ObjectDisposedException.ThrowIf(disposedValue, this);
#else
        if (disposedValue) throw new ObjectDisposedException(nameof(ConnectionPool));
#endif
        T? item = _fastItem;

        if (item != null && Interlocked.CompareExchange(ref _fastItem, null, item) == item)
        {
            if (ValidateObject(item))
            {
                Debug.WriteLine("借用了连接-快速对象");
                return item;
            }
            item.Dispose();
            return CreateNewConnection();
        }
        while (_items.TryDequeue(out var ci))
        {
            item = ci.Object;
            Debug.WriteLine("借用了连接-队列");
            Interlocked.Decrement(ref _numItems);
            if (ValidateObject(item))
                return item;

            item.Dispose();
        }

        return CreateNewConnection();
    }

    public virtual void Return(T item)
    {
        if (disposedValue)
        {
            item.Dispose();
            return;
        }
        if (!ValidateObject(item))
        {
            item.Dispose();
            return;
        }
        // 快速路径尝试
        if (_fastItem == null && Interlocked.CompareExchange(ref _fastItem, item, null) == null)
        {
            Debug.WriteLine("归还了对象-快速对象");
            return;
        }
        if (Interlocked.Increment(ref _numItems) <= _maxCapacity)
        {
            _items.Enqueue(new(item));
            Debug.WriteLine("归还了对象-队列");
            return;
        }
        Interlocked.Decrement(ref _numItems);
        HandleOverflowObject(item);
    }

    private void HealthCheckCallback(object? state)
    {
        if (disposedValue) return;

        lock (_maintenanceLock)
        {
            HealchCheck();
        }
    }
    private T CreateNewConnection()
    {
        Debug.WriteLine("创建了新的对象");
        return _createFunc();
    }
    protected virtual void HealchCheck()
    {

    }
    protected abstract void HandleOverflowObject(T item);
    protected virtual bool ValidateObject(T item) => true;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _healthCheckTimer.Dispose();
                while (_items.TryDequeue(out var i))
                {
                    i.Object.Dispose();
                }
                _fastItem?.Dispose();
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
