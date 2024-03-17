using System.Collections.Generic;

namespace LightORM.Interfaces;

public interface IExpDelete<T> : ISql<IExpDelete<T>, T>, ITransactionable
{
    IExpDelete<T> AppendData(T item);
    IExpDelete<T> Where(IEnumerable<T> items);
}
