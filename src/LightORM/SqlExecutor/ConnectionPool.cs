using System.Data.Common;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;
namespace LightORM.SqlExecutor;

internal class ConnectionPool : IDisposable
{
    private readonly Func<DbConnection> _createFunc;
    private readonly int _maxCapacity;
    private int _numItems;
    private protected readonly ConcurrentQueue<ConnectionInfo> _items = new();
    private protected DbConnection? _fastItem;
    private bool disposedValue;
    private readonly Timer _healthCheckTimer;
    private readonly TimeSpan _connectionLifetime = TimeSpan.FromMinutes(15);
    private readonly object _maintenanceLock = new();
    public readonly struct ConnectionInfo(DbConnection connection)
    {
        public DbConnection Connection { get; } = connection;
        public DateTimeOffset Created { get; } = DateTimeOffset.Now;
    }
    public ConnectionPool(Func<DbConnection> func, int maxCapacity)
    {
        _createFunc = func;
        _maxCapacity = maxCapacity;
        _healthCheckTimer = new Timer(HealthCheckCallback, null,
            TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }
    public DbConnection Get()
    {
#if NET8_0_OR_GREATER
        ObjectDisposedException.ThrowIf(disposedValue, this);
#else
        if (disposedValue) throw new ObjectDisposedException(nameof(ConnectionPool));
#endif
        DbConnection? item = _fastItem;

        if (item != null && Interlocked.CompareExchange(ref _fastItem, null, item) == item)
        {
            if (ValidateConnection(item))
            {
                Debug.WriteLine("借用了连接-快速对象");
                return item;
            }
            item.Dispose();
            return CreateNewConnection();
        }
        while (_items.TryDequeue(out var ci))
        {
            item = ci.Connection;
            Debug.WriteLine("借用了连接-队列");
            Interlocked.Decrement(ref _numItems);
            if (ValidateConnection(item))
                return item;

            item.Dispose();
        }

        return CreateNewConnection();
    }

    public void Return(DbConnection connection)
    {
        if (disposedValue)
        {
            connection.Dispose();
            return;
        }
        if (!ValidateConnection(connection))
        {
            connection.Dispose();
            return;
        }
        // 快速路径尝试
        if (_fastItem == null && Interlocked.CompareExchange(ref _fastItem, connection, null) == null)
        {
            Debug.WriteLine("归还了连接-快速对象");
            return;
        }
        if (Interlocked.Increment(ref _numItems) <= _maxCapacity)
        {
            _items.Enqueue(new(connection));
            Debug.WriteLine("归还了连接-队列");
            return;
        }
        Interlocked.Decrement(ref _numItems);
        SmartRecycleConnection(connection);
    }
    private DbConnection CreateNewConnection() => _createFunc();

    private static bool ValidateConnection(DbConnection connection)
    {
        return true;
        //try
        //{
        //    if (connection.State != ConnectionState.Open)
        //        connection.Open();

        //    // 简单健康检查 (例如SQLite使用PRAGMA quick_check)
        //    using var cmd = connection.CreateCommand();
        //    cmd.CommandText = "SELECT 1";
        //    return cmd.ExecuteScalar()?.ToString() == "1";
        //}
        //catch
        //{
        //    return false;
        //}
    }
    private static void SmartRecycleConnection(DbConnection connection)
    {
        // 根据系统内存压力决定处理方式
        if (IsSystemUnderMemoryPressure())
        {
            connection.Dispose();
        }
        else
        {
            // 延迟关闭策略
            Task.Delay(5000).ContinueWith(_ =>
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                connection.Dispose();
            });
        }
        static bool IsSystemUnderMemoryPressure()
        {
            // 实际实现应根据具体平台调整
            try
            {
                var process = Process.GetCurrentProcess();
                return process.PrivateMemorySize64 > 500 * 1024 * 1024; // 500MB阈值
            }
            catch
            {
                return false;
            }
        }
    }

    private void HealthCheckCallback(object? state)
    {
        if (disposedValue) return;

        lock (_maintenanceLock)
        {
            // 清理过期连接
            var tempList = new List<ConnectionInfo>();
            while (_items.TryDequeue(out var conn))
            {
                if (IsConnectionExpired(conn))
                {
                    conn.Connection.Dispose();
                }
                else
                {
                    tempList.Add(conn);
                }
            }

            // 重建队列
            foreach (var conn in tempList)
                _items.Enqueue(conn);
        }
        bool IsConnectionExpired(ConnectionInfo connection)
        {
            return (DateTimeOffset.Now - connection.Created) > _connectionLifetime;
        }
    }

    //private static void Connection_StateChange(object sender, StateChangeEventArgs e)
    //{
    //    if (e.CurrentState == ConnectionState.Closed)
    //    {
    //        var conn = (DbConnection)sender;
    //        conn.StateChange -= Connection_StateChange;
    //        conn.Dispose();
    //    }
    //}

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _healthCheckTimer.Dispose();
                while (_items.TryDequeue(out var i))
                {
                    i.Connection.Dispose();
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
