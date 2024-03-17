using System.Threading.Tasks;

namespace LightORM.Providers;

internal sealed class InsertProvider<T> : IExpInsert<T>
{
    private readonly ISqlExecutor executor;

    public InsertProvider(ISqlExecutor executor, T? entity)
    {
        this.executor = executor;
    }
    public IExpInsert<T> AppendData(T item)
    {
        throw new NotImplementedException();
    }

    public IExpInsert<T> AppendData(IEnumerable<T> items)
    {
        throw new NotImplementedException();
    }

    public IExpInsert<T> AttachCancellationToken(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public int Execute()
    {
        throw new NotImplementedException();
    }

    public Task<int> ExecuteAsync()
    {
        throw new NotImplementedException();
    }

    public IExpInsert<T> IgnoreColumns(Expression<Func<T, object>> columns)
    {
        throw new NotImplementedException();
    }

    public IExpInsert<T> SetColumns(Expression<Func<T, object>> columns)
    {
        throw new NotImplementedException();
    }

    public string ToSql()
    {
        throw new NotImplementedException();
    }
}
