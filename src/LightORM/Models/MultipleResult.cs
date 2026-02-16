using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Models;

public class MultipleResult(DbDataReader reader) : IDisposable
{
    private int currentResultSet = 0;
    private bool disposedValue;

    private void Check()
    {
        if (disposedValue) throw new ObjectDisposedException(nameof(MultipleResult));

        if (currentResultSet > 0)
        {
            if (!reader.NextResult())
                throw new InvalidOperationException("No more result sets available");
        }
        currentResultSet++;
    }

    public IEnumerable<T> Read<T>()
    {
        Check();
        Func<IDataReader, T> deserializer = ExpressionBuilder.BuildDeserializer<T>(reader);
        while (reader.Read())
        {
            yield return deserializer(reader);
        }
    }

    public async IAsyncEnumerable<T> ReadAsync<T>()
    {
        Check();
        Func<IDataReader, T> deserializer = ExpressionBuilder.BuildDeserializer<T>(reader);
        while (await reader.ReadAsync())
        {
            yield return deserializer(reader);
        }
    }

    public async Task<IList<T>> ReadListAsync<T>()
    {
        Check();
        Func<IDataReader, T> deserializer = ExpressionBuilder.BuildDeserializer<T>(reader);
        List<T> results = [];
        while (await reader.ReadAsync())
        {
            var r = deserializer(reader);
            results.Add(r);
        }
        return results;
    }

    public T? ReadFirst<T>()
    {
        Check();
        if (reader.Read())
        {
            var deserializer = ExpressionBuilder.BuildDeserializer<T>(reader);
            return deserializer(reader);
        }

        return default;
    }

    public async Task<T?> ReadFirstAsync<T>()
    {
        Check();
        if (await reader.ReadAsync())
        {
            var deserializer = ExpressionBuilder.BuildDeserializer<T>(reader);
            return deserializer(reader);
        }

        return default;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                reader?.Close();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    //public  ValueTask DisposeAsync()
    //{
    //    if (!disposed)
    //    {
    //        reader.Close();
    //        disposed = true;
    //    }
    //}
}

