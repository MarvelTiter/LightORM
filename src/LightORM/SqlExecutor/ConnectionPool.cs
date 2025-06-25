using System.Data.Common;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;
using static LightORM.SqlExecutor.ConnectionPool;
namespace LightORM.SqlExecutor;
internal class ConnectionPool(Func<DbConnection> func, int maxCapacity) : ObjectPool<DbConnection>(func,maxCapacity)
{
    private readonly TimeSpan _connectionLifetime = TimeSpan.FromMinutes(15);

    protected override void HandleOverflowObject(DbConnection item)
    {
        // 根据系统内存压力决定处理方式
        if (IsSystemUnderMemoryPressure())
        {
            item.Dispose();
        }
        else
        {
            // 延迟关闭策略
            Task.Delay(5000).ContinueWith(_ =>
            {
                if (item.State == ConnectionState.Open)
                    item.Close();
                item.Dispose();
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

    protected override void HealchCheck()
    {
        // 清理过期连接
        var tempList = new List<ObjectInfo>();
        while (_items.TryDequeue(out var conn))
        {
            if (IsConnectionExpired(conn))
            {
                conn.Object.Dispose();
            }
            else
            {
                tempList.Add(conn);
            }
        }

        // 重建队列
        foreach (var conn in tempList)
            _items.Enqueue(conn);
        bool IsConnectionExpired(ObjectInfo connection)
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
}
