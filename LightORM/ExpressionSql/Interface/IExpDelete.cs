using System.Collections.Generic;

namespace MDbContext.ExpressionSql.Interface
{
    public interface IExpDelete<T> : ISql<IExpDelete<T>, T>, ITransactionable
    {
        IExpDelete<T> AppendData(T item);
        IExpDelete<T> Where(IEnumerable<T> items);
    }
}
