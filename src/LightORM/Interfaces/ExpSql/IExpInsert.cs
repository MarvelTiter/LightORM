using System.Threading;

namespace LightORM.Interfaces.ExpSql;

public interface IExpInsert<T> : ISql<IExpInsert<T>, T>
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
    /// <summary>
    /// 如果存在则更新，否则插入
    /// </summary>
    /// <typeparam name="Columns"></typeparam>
    /// <param name="where"></param>
    /// <param name="columns"></param>
    /// <returns></returns>
    IExpInsert<T> OrUpdate<Columns>(Expression<Func<T, bool>>? where, Expression<Func<T,Columns>>? columns);
    /// <summary>
    /// 如果存在则忽略
    /// </summary>
    /// <returns></returns>
    IExpInsert<T> IgnoreIfExits();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
}
