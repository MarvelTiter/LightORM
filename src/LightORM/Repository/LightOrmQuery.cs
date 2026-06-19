using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace LightORM.Repository;

internal class LightOrmQuery<
#if NET8_0_OR_GREATER
//[UnconditionalSuppressMessage("AOT", "IL2091", Justification = "LightOrmQueryProvider的Execute<T>AOT有风险")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
T>(IQueryProvider queryProvider) : IOrderedQueryable<T>
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
