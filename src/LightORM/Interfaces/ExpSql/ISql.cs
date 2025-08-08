using System.Threading;
namespace LightORM.Interfaces.ExpSql;

public interface ISql
{
    string ToSql();
    string ToSqlWithParameters();
}
public interface ISql<TPart, T> : ISql
{
    TPart Where(Expression<Func<T, bool>> exp);
    TPart WhereIf(bool condition, Expression<Func<T, bool>> exp);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
}
