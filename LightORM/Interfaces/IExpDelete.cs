namespace LightORM.Interfaces;

public interface IExpDelete<T> : ISql<IExpDelete<T>, T>
{
    IExpDelete<T> AppendData(T item);
    IExpDelete<T> Where(IEnumerable<T> items);
}
