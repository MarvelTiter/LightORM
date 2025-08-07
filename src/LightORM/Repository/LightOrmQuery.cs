using System.Data.Common;

namespace LightORM.Repository;

internal class LightOrmQuery<T>(IQueryProvider queryProvider) : IOrderedQueryable<T>
{
    private readonly IQueryProvider _queryProvider = queryProvider ?? throw new ArgumentNullException(nameof(queryProvider));

    public Type ElementType => typeof(T);
    public Expression Expression => Expression.Constant(this);
    public IQueryProvider Provider => _queryProvider;
    public IEnumerator<T> GetEnumerator()
    {
        var reader = Provider.Execute(Expression) as DbDataReader;
        if (reader == null)
        {
            throw new InvalidOperationException("The executed expression did not return a DbDataReader.");
        }
        try
        {
            var func = reader.BuildDeserializer<T>();
            while (reader.Read())
            {
                yield return (T)func(reader);
            }
        }
        finally
        {
            reader.Dispose();
        }
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
