using System.Threading;

namespace LightORM.Interfaces.ExpSql;

public interface IExpInsert<T> : ISql
{
    //IExpInsert<T> AppendData(T item);
    //IExpInsert<T> AppendData(IEnumerable<T> items);
    IExpInsert<T> SetColumns<TSet>(Expression<Func<T, TSet>> columns);
    IExpInsert<T> IgnoreColumns<TIgnore>(Expression<Func<T, TIgnore>> columns);
    int Execute();
    internal void SetTargetObject(T? entity);
    //IExpInsert<T> NoParameter();
    IExpInsert<T> ReturnIdentity();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
}
