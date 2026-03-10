using System.Threading;

namespace LightORM.Interfaces.ExpSql;

public interface IExpInsert<T> : ISql
{
    //IExpInsert<T> AppendData(T item);
    //IExpInsert<T> AppendData(IEnumerable<T> items);
    IExpInsert<T> InsertColumns<TColumns>(Expression<Func<T, TColumns>> columns);
    IExpInsert<T> IgnoreColumns<TIgnore>(Expression<Func<T, TIgnore>> columns);
    IExpInsert<T> Set<TField>(Expression<Func<T, TField>> field, TField value);
    IExpInsert<T> SetIf<TField>(bool condition, Expression<Func<T, TField>> field, TField value);
    IExpInsert<T> InsertByName(string propertyName, object? value = null);
    IExpInsert<T> InsertByNames(string[] propertyNames, object[]? values = null);
    int Execute();
    internal void SetTargetObject(T? entity);
    //IExpInsert<T> NoParameter();
    IExpInsert<T> ReturnIdentity();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
}
