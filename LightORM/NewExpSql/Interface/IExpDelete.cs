using System.Collections.Generic;

namespace MDbContext.NewExpSql.Interface
{
    public interface IExpDelete<T> : ISql<IExpDelete<T>, T>
    {
        IExpDelete<T> Where(T item);
        IExpDelete<T> Where(IEnumerable<T> items);
    }
}
